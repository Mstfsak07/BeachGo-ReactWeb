from __future__ import annotations

import argparse
import json
import os
import re
import shutil
import subprocess
import sys
import textwrap
from dataclasses import asdict, dataclass
from datetime import datetime, timezone
from pathlib import Path
from typing import Any

try:
    import tomllib
except ModuleNotFoundError:  # pragma: no cover
    tomllib = None


@dataclass
class CommandResult:
    name: str
    command: list[str]
    cwd: str
    stdout_path: str
    stderr_path: str
    exit_code: int | None
    status: str
    message: str


@dataclass
class TaskResult:
    task_id: str
    title: str
    executor: str
    attempt: int
    status: str
    stdout_path: str
    stderr_path: str
    exit_code: int | None
    message: str


@dataclass
class ModelSelection:
    model: str
    escalation_level: str
    reason: str


def utc_now() -> str:
    return datetime.now(timezone.utc).isoformat()


def read_text(path: Path) -> str:
    return path.read_text(encoding="utf-8-sig")


def write_text(path: Path, content: str) -> None:
    path.parent.mkdir(parents=True, exist_ok=True)
    path.write_text(content, encoding="utf-8")


def write_json(path: Path, payload: Any) -> None:
    path.parent.mkdir(parents=True, exist_ok=True)
    path.write_text(json.dumps(payload, indent=2, ensure_ascii=False), encoding="utf-8")


def slugify(value: str) -> str:
    chars: list[str] = []
    for char in value.lower():
        if char.isalnum():
            chars.append(char)
        elif chars and chars[-1] != "-":
            chars.append("-")
    return "".join(chars).strip("-") or "item"


def is_relative_to(path: Path, parent: Path) -> bool:
    try:
        path.resolve().relative_to(parent.resolve())
        return True
    except ValueError:
        return False


class WorkspaceGuard:
    def __init__(self, workspace_root: Path) -> None:
        self.workspace_root = workspace_root.resolve()

    def assert_write_path(self, path: Path) -> None:
        if not is_relative_to(path, self.workspace_root):
            raise RuntimeError(f"Refusing to write outside workspace: {path}")


class Config:
    def __init__(self, repo_root: Path) -> None:
        self.repo_root = repo_root
        self.path = repo_root / ".codex" / "config.toml"
        self.data = self._load()

    def _load(self) -> dict[str, Any]:
        if not self.path.exists():
            return {}
        if tomllib is None:
            raise RuntimeError("Python 3.11+ is required for TOML config parsing.")
        with self.path.open("rb") as handle:
            return tomllib.load(handle)

    def orchestrator(self) -> dict[str, Any]:
        return self.data.get("orchestrator", {})

    def executor_command(self, executor: str) -> list[str]:
        entry = self.data.get("executors", {}).get(executor, {})
        command = entry.get("command")
        if not command:
            raise RuntimeError(f"Missing executor command template for '{executor}' in {self.path}")
        return list(command)

    def model_policy(self) -> dict[str, Any]:
        return self.data.get("model_policy", {})


class ReviewOrchestrator:
    def __init__(self, args: argparse.Namespace) -> None:
        self.repo_root = Path(__file__).resolve().parents[1]
        self.guard = WorkspaceGuard(self.repo_root)
        self.config = Config(self.repo_root)
        cfg = self.config.orchestrator()

        self.mode = args.mode or cfg.get("default_mode", "dry-run")
        self.max_phase_retries = args.max_phase_retries or int(cfg.get("max_phase_retries", 3))
        self.planner_executor = args.planner_executor or cfg.get("planner_executor", "local")
        self.state_dir = (self.repo_root / cfg.get("state_dir", "ops/state")).resolve()
        self.prompt_dir = (self.repo_root / cfg.get("prompt_dir", "ops/prompts")).resolve()
        self.log_dir = (self.repo_root / cfg.get("log_dir", "ops/state/logs")).resolve()
        assessment_file = args.assessment_file or cfg.get("default_assessment_file")
        if not assessment_file:
            raise RuntimeError("Assessment file must be provided either as an argument or in config.")
        self.assessment_path = Path(assessment_file).resolve()
        self.run_id = datetime.now().strftime("%Y%m%d-%H%M%S")
        self.run_dir = self.state_dir / self.run_id
        self.state_summary_limit = int(cfg.get("state_summary_limit", 3))
        self.planner_timeout_seconds = int(cfg.get("planner_timeout_seconds", 45))
        self.executor_timeout_seconds = int(cfg.get("executor_timeout_seconds", 180))
        self.verification_timeout_seconds = int(cfg.get("verification_timeout_seconds", 900))
        self.model_policy = self.config.model_policy()
        self.phase_model_usage: dict[str, dict[str, bool]] = {}

        self.guard.assert_write_path(self.run_dir)
        self.run_dir.mkdir(parents=True, exist_ok=True)
        self.log_dir.mkdir(parents=True, exist_ok=True)
        self.repo_summary = self.detect_repo_summary()

    def fail(self, message: str, details: dict[str, Any] | None = None) -> None:
        write_json(
            self.run_dir / "error.json",
            {
                "status": "error",
                "message": message,
                "details": details or {},
                "timestamp": utc_now(),
            },
        )
        raise RuntimeError(message)

    def ensure_prerequisites(self) -> None:
        detected: dict[str, str] = {}
        missing: list[str] = []
        for binary in ("claude", "gemini"):
            resolved = shutil.which(binary)
            if resolved:
                detected[binary] = resolved
            else:
                missing.append(binary)
        payload = {"detected": detected, "missing": missing, "timestamp": utc_now()}
        write_json(self.run_dir / "cli_check.json", payload)
        if missing:
            self.fail(f"Required CLI binaries are missing: {', '.join(missing)}", payload)

    def detect_repo_summary(self) -> dict[str, Any]:
        summary: dict[str, Any] = {
            "root": str(self.repo_root),
            "node_projects": [],
            "python_projects": [],
            "dotnet_projects": [],
        }

        for package_json in self.repo_root.rglob("package.json"):
            if "node_modules" in package_json.parts:
                continue
            try:
                package_data = json.loads(read_text(package_json))
            except json.JSONDecodeError:
                continue
            summary["node_projects"].append(
                {
                    "path": str(package_json.parent.relative_to(self.repo_root)),
                    "name": package_data.get("name") or package_json.parent.name,
                    "scripts": package_data.get("scripts", {}),
                }
            )

        python_markers = ("pyproject.toml", "setup.py", "requirements.txt")
        seen_python: set[str] = set()
        for marker in python_markers:
            for path in self.repo_root.rglob(marker):
                rel = str(path.parent.relative_to(self.repo_root))
                if rel not in seen_python:
                    seen_python.add(rel)
                    summary["python_projects"].append(rel)

        for path in self.repo_root.rglob("*"):
            if path.suffix.lower() in {".sln", ".slnx", ".csproj"}:
                summary["dotnet_projects"].append(str(path.relative_to(self.repo_root)))

        return summary

    def prompt_text(self, template_name: str, body: str) -> str:
        template_path = self.prompt_dir / template_name
        if not template_path.exists():
            self.fail(f"Prompt template missing: {template_path}")
        return f"{read_text(template_path).strip()}\n\n{body.strip()}\n"

    def load_phase_state_summary(self) -> list[dict[str, Any]]:
        summaries: list[dict[str, Any]] = []
        for path in sorted(self.state_dir.glob("phase-*.json"))[-self.state_summary_limit :]:
            try:
                payload = json.loads(read_text(path))
            except json.JSONDecodeError:
                continue
            summaries.append(
                {
                    "phase_id": payload.get("phase_id"),
                    "title": payload.get("title"),
                    "status": payload.get("status"),
                    "attempts_used": payload.get("attempts_used"),
                }
            )
        return summaries

    def describe_task_size(self, task: dict[str, Any]) -> str:
        text = " ".join(
            [
                str(task.get("title", "")),
                str(task.get("description", "")),
                " ".join(task.get("areas", [])),
                str(task.get("reason", "")),
            ]
        ).lower()
        large_signals = ("refactor", "architecture", "cross-file", "migration", "complex", "risky", "hard")
        return "large" if any(signal in text for signal in large_signals) else "small"

    def select_model(self, phase: dict[str, Any], task: dict[str, Any], purpose: str) -> ModelSelection:
        phase_usage = self.phase_model_usage.setdefault(phase["phase_id"], {"large_used": False})
        default_model = self.model_policy.get("default", "gpt-5.4-mini")
        code_model = self.model_policy.get("code_execution", "gpt-5.3-codex")
        large_model = self.model_policy.get("escalation", "gpt-5.4")
        task_size = self.describe_task_size(task)

        if purpose == "repair":
            if phase_usage["large_used"]:
                return ModelSelection(code_model, "fallback", "Large model already used in this phase; using code model.")
            phase_usage["large_used"] = True
            return ModelSelection(large_model, "escalated", "Verification repair may require stronger reasoning.")

        if purpose == "planning":
            return ModelSelection(default_model, "default", "Planner starts with the default small model policy.")

        if task_size == "small":
            return ModelSelection(code_model, "code", "Small task stays on the smaller execution model.")

        if not phase_usage["large_used"]:
            phase_usage["large_used"] = True
            return ModelSelection(large_model, "escalated", "Large task escalated once for the phase.")

        return ModelSelection(code_model, "code", "Large model already used in this phase; avoiding repeated escalation.")

    def select_runtime_model(self, executor: str, phase: dict[str, Any], task: dict[str, Any], purpose: str) -> str | None:
        if executor != "gemini":
            return None
        task_size = self.describe_task_size(task)
        gemini_default = self.model_policy.get("gemini_default", "gemini-3-flash")
        gemini_escalation = self.model_policy.get("gemini_escalation", "gemini-3-pro")
        if purpose == "repair":
            phase_usage = self.phase_model_usage.setdefault(phase["phase_id"], {"large_used": False})
            return gemini_default if phase_usage["large_used"] else gemini_escalation
        if purpose == "planning":
            return gemini_default
        return gemini_escalation if task_size == "large" else gemini_default

    def invoke_executor(
        self,
        executor: str,
        prompt: str,
        model: str | None,
        stdout_path: Path,
        stderr_path: Path,
        cwd: Path,
        timeout_seconds: int,
    ) -> CommandResult:
        self.guard.assert_write_path(stdout_path)
        self.guard.assert_write_path(stderr_path)
        stdout_path.parent.mkdir(parents=True, exist_ok=True)
        stderr_path.parent.mkdir(parents=True, exist_ok=True)
        raw_command = self.config.executor_command(executor)
        command_without_prompt: list[str] = []
        skip_next = False
        for index, part in enumerate(raw_command):
            if skip_next:
                skip_next = False
                continue
            if part == "{prompt}":
                continue
            if part in {"-p", "--prompt"} and index + 1 < len(raw_command) and raw_command[index + 1] == "{prompt}":
                skip_next = True
                continue
            command_without_prompt.append(part.format(model=model or "", prompt=""))

        resolved = shutil.which(command_without_prompt[0]) or command_without_prompt[0]
        command_without_prompt[0] = resolved
        prompt_path = stdout_path.parent / "prompt.txt"
        self.guard.assert_write_path(prompt_path)
        write_text(prompt_path, prompt)

        compact_prompt = re.sub(r"\s+", " ", prompt).strip()
        if os.name == "nt":
            compact_prompt_path = stdout_path.parent / "prompt.compact.txt"
            self.guard.assert_write_path(compact_prompt_path)
            write_text(compact_prompt_path, compact_prompt)
            pwsh_args = ", ".join("'" + arg.replace("'", "''") + "'" for arg in command_without_prompt[1:])
            script = textwrap.dedent(
                f"""
                $taskPrompt = Get-Content -LiteralPath '{str(compact_prompt_path).replace("'", "''")}' -Raw -Encoding UTF8
                $exePath = '{str(resolved).replace("'", "''")}'
                $cliArgs = @({pwsh_args})
                & $exePath @cliArgs -p $taskPrompt
                exit $LASTEXITCODE
                """
            ).strip()
            command = ["powershell", "-NoProfile", "-ExecutionPolicy", "Bypass", "-Command", script]
        else:
            command = [resolved, *command_without_prompt[1:], "-p", compact_prompt]

        if self.mode == "dry-run":
            write_text(stdout_path, f"[dry-run] {' '.join(command)}\n")
            write_text(stderr_path, "")
            return CommandResult(
                name=executor,
                command=command,
                cwd=str(cwd),
                stdout_path=str(stdout_path),
                stderr_path=str(stderr_path),
                exit_code=0,
                status="dry-run",
                message="Command not executed because mode=dry-run",
            )

        try:
            completed = subprocess.run(
                command,
                cwd=str(cwd),
                text=True,
                encoding="utf-8",
                errors="replace",
                capture_output=True,
                shell=False,
                env=os.environ.copy(),
                timeout=timeout_seconds,
            )
        except subprocess.TimeoutExpired as exc:
            write_text(stdout_path, exc.stdout or "")
            write_text(stderr_path, exc.stderr or f"Executor timeout after {timeout_seconds} seconds")
            return CommandResult(
                name=executor,
                command=command,
                cwd=str(cwd),
                stdout_path=str(stdout_path),
                stderr_path=str(stderr_path),
                exit_code=None,
                status="timeout",
                message=f"{executor} timed out after {timeout_seconds} seconds",
            )
        write_text(stdout_path, completed.stdout or "")
        write_text(stderr_path, completed.stderr or "")
        status = "success" if completed.returncode == 0 else "failed"
        return CommandResult(
            name=executor,
            command=command,
            cwd=str(cwd),
            stdout_path=str(stdout_path),
            stderr_path=str(stderr_path),
            exit_code=completed.returncode,
            status=status,
            message=f"{executor} exited with code {completed.returncode}",
        )

    def safe_parse_json_response(self, raw: str) -> dict[str, Any] | None:
        start = raw.find("{")
        end = raw.rfind("}")
        if start == -1 or end == -1 or end < start:
            return None
        try:
            return json.loads(raw[start : end + 1])
        except json.JSONDecodeError:
            return None

    def is_unusable_executor_output(self, output: str) -> bool:
        normalized = output.strip().lower()
        return normalized in {
            "i'm ready to help with your repository. what would you like to work on?",
            "i'm ready to help you with your beachgo reactweb project. what would you like to work on?",
            "i'm ready to help with your beachgo react web project. what would you like to work on?",
        }

    def has_valid_worker_result(self, output: str) -> bool:
        stripped = output.strip()
        if not stripped:
            return False
        first_line = stripped.splitlines()[0].strip()
        return first_line in {"RESULT: OK", "RESULT: FAIL"}

    def parse_json_response(self, raw: str) -> dict[str, Any]:
        parsed = self.safe_parse_json_response(raw)
        if parsed is None:
            self.fail("Planner output was not valid JSON.", {"output": raw})
        return parsed

    def infer_phase_bucket(self, line: str) -> tuple[str, str]:
        lower = line.lower()
        if any(keyword in lower for keyword in ("kritik", "critical", "blocker", "sqlite", "postgresql", "mockpayment")):
            return "phase-1", "Critical And Blockers"
        if any(keyword in lower for keyword in ("security", "xss", "cookie", "localstorage", "brute-force", "iptal", "token")):
            return "phase-2", "Correctness And Security"
        if any(keyword in lower for keyword in ("yapı", "refactor", "domain", "controller", "docker-compose", "ci/cd", "gelir")):
            return "phase-3", "Maintainability And Refactor"
        return "phase-4", "Tests Docs Cleanup"

    def infer_executor(self, text: str) -> str:
        lower = text.lower()
        if any(keyword in lower for keyword in ("sqlite", "postgresql", "mockpayment", "controller", "domain", "docker-compose", "refactor")):
            return "claude"
        return "gemini"

    def generate_local_plan(self, assessment_text: str) -> dict[str, Any]:
        phases: dict[str, dict[str, Any]] = {
            "phase-1": {"phase_id": "phase-1", "title": "Critical And Blockers", "goal": "Resolve release-blocking issues first.", "depends_on": [], "tasks": []},
            "phase-2": {"phase_id": "phase-2", "title": "Correctness And Security", "goal": "Fix correctness and security defects after blockers.", "depends_on": ["phase-1"], "tasks": []},
            "phase-3": {"phase_id": "phase-3", "title": "Maintainability And Refactor", "goal": "Clean structural issues without expanding scope.", "depends_on": ["phase-2"], "tasks": []},
            "phase-4": {"phase_id": "phase-4", "title": "Tests Docs Cleanup", "goal": "Finish validation, tests, docs, and cleanup.", "depends_on": ["phase-3"], "tasks": []},
        }
        lines = [line.strip() for line in assessment_text.splitlines()]
        findings = [line for line in lines if line and line[0].isdigit() and ". " in line]
        for index, finding in enumerate(findings, start=1):
            phase_id, _ = self.infer_phase_bucket(finding)
            executor = self.infer_executor(finding)
            phases[phase_id]["tasks"].append(
                {
                    "task_id": f"{phase_id}-task-{len(phases[phase_id]['tasks']) + 1}",
                    "title": finding.split("**")[1] if "**" in finding else f"Assessment item {index}",
                    "description": finding,
                    "executor": executor,
                    "reason": "Local fallback planner assigned executor from issue scope.",
                    "depends_on": [],
                    "areas": ["backend" if "api" in finding.lower() or "token" in finding.lower() else "repo"],
                }
            )
        ordered = [phases["phase-1"], phases["phase-2"], phases["phase-3"], phases["phase-4"]]
        return {"phases": [phase for phase in ordered if phase["tasks"]]}

    def create_plan(self, assessment_text: str) -> dict[str, Any]:
        planning_phase = {"phase_id": "planning"}
        if self.planner_executor == "local":
            plan = self.generate_local_plan(assessment_text)
            write_json(
                self.run_dir / "planner_result.json",
                {
                    "name": "local",
                    "status": "success",
                    "message": "Local planner generated the phase plan.",
                    "timestamp": utc_now(),
                },
            )
            write_json(self.run_dir / "planner_local_fallback.json", plan)
            write_json(self.run_dir / "plan.json", plan)
            return plan
        model = self.select_model(planning_phase, {}, "planning")
        prompt = self.prompt_text(
            "phase_planner.md",
            textwrap.dedent(
                f"""
                Phase state summary:
                {json.dumps(self.load_phase_state_summary(), indent=2)}

                Model selection:
                {json.dumps(asdict(model), indent=2)}

                Repository summary:
                {json.dumps(self.repo_summary, indent=2)}

                Assessment text:
                {assessment_text}
                """
            ),
        )
        result = self.invoke_executor(
            self.planner_executor,
            prompt,
            self.select_runtime_model(self.planner_executor, planning_phase, {}, "planning"),
            self.run_dir / "planner.stdout.log",
            self.run_dir / "planner.stderr.log",
            self.repo_root,
            self.planner_timeout_seconds,
        )
        write_json(self.run_dir / "planner_result.json", asdict(result))

        if self.mode == "dry-run":
            plan = {
                "phases": [
                    {
                        "phase_id": "phase-1",
                        "title": "Assessment Triage",
                        "goal": "Dry-run placeholder plan because the live planner was not executed.",
                        "depends_on": [],
                        "tasks": [
                            {
                                "task_id": "phase-1-task-1",
                                "title": "Inspect assessment and prepare live execution",
                                "description": "Review the generated dry-run artifacts, then rerun with --execute.",
                                "executor": "gemini",
                                "reason": "Safe placeholder for dry-run.",
                                "depends_on": [],
                                "areas": ["planning"],
                            }
                        ],
                    }
                ]
            }
        else:
            raw_output = read_text(self.run_dir / "planner.stdout.log")
            parsed = self.safe_parse_json_response(raw_output)
            if (parsed is None or result.status in {"failed", "timeout"}) and self.planner_executor != "gemini":
                fallback_result = self.invoke_executor(
                    "gemini",
                    prompt,
                    self.select_runtime_model("gemini", planning_phase, {}, "planning"),
                    self.run_dir / "planner_fallback.stdout.log",
                    self.run_dir / "planner_fallback.stderr.log",
                    self.repo_root,
                    self.planner_timeout_seconds,
                )
                write_json(self.run_dir / "planner_fallback_result.json", asdict(fallback_result))
                raw_output = read_text(self.run_dir / "planner_fallback.stdout.log")
                parsed = self.safe_parse_json_response(raw_output)
            if parsed is None:
                parsed = self.generate_local_plan(assessment_text)
                write_json(self.run_dir / "planner_local_fallback.json", parsed)
            plan = parsed
            if not isinstance(plan.get("phases"), list):
                self.fail("Planner JSON did not include a valid 'phases' list.", {"plan": plan})

        write_json(self.run_dir / "plan.json", plan)
        return plan

    def topological_tasks(self, tasks: list[dict[str, Any]]) -> list[dict[str, Any]]:
        by_id = {task["task_id"]: task for task in tasks}
        ordered: list[dict[str, Any]] = []
        pending = set(by_id)
        while pending:
            progressed = False
            for task_id in list(pending):
                deps = set(by_id[task_id].get("depends_on", []))
                if deps.issubset({task["task_id"] for task in ordered}):
                    ordered.append(by_id[task_id])
                    pending.remove(task_id)
                    progressed = True
            if not progressed:
                self.fail("Task dependencies contain a cycle or unknown dependency.", {"tasks": tasks})
        return ordered

    def run_phase_task(self, phase: dict[str, Any], task: dict[str, Any], attempt: int) -> TaskResult:
        executor = task["executor"]
        if executor not in {"claude", "gemini"}:
            self.fail(f"Unsupported executor '{executor}' in task plan.", {"task": task})

        template = "claude_worker.md" if executor == "claude" else "gemini_worker.md"
        model = self.select_model(phase, task, "task")
        prompt = self.prompt_text(
            template,
            textwrap.dedent(
                f"""
                Phase state summary:
                {json.dumps(self.load_phase_state_summary(), indent=2)}

                Model selection:
                {json.dumps(asdict(model), indent=2)}

                Repository root: {self.repo_root}
                Phase: {phase['phase_id']} - {phase['title']}
                Phase goal: {phase.get('goal', '')}
                Task id: {task['task_id']}
                Task title: {task['title']}
                Task description: {task['description']}
                Executor reason: {task.get('reason', '')}
                Areas: {', '.join(task.get('areas', []))}
                Attempt: {attempt}

                Apply the task directly in the repository and keep all writes inside the workspace.
                """
            ),
        )

        task_dir = self.log_dir / phase["phase_id"] / task["task_id"] / f"attempt-{attempt}"
        result = self.invoke_executor(
            executor,
            prompt,
            self.select_runtime_model(executor, phase, task, "task"),
            task_dir / "stdout.log",
            task_dir / "stderr.log",
            self.repo_root,
            self.executor_timeout_seconds,
        )
        if executor == "claude" and result.status == "success":
            raw_output = read_text(task_dir / "stdout.log")
            if self.is_unusable_executor_output(raw_output):
                fallback_result = self.invoke_executor(
                    "gemini",
                    prompt,
                    self.select_runtime_model("gemini", phase, task, "task"),
                    task_dir / "fallback.stdout.log",
                    task_dir / "fallback.stderr.log",
                    self.repo_root,
                    self.executor_timeout_seconds,
                )
                if fallback_result.status == "success":
                    result = CommandResult(
                        name="gemini-fallback",
                        command=fallback_result.command,
                        cwd=fallback_result.cwd,
                        stdout_path=fallback_result.stdout_path,
                        stderr_path=fallback_result.stderr_path,
                        exit_code=fallback_result.exit_code,
                        status=fallback_result.status,
                        message="Claude returned unusable output; task rerun with Gemini fallback.",
                    )
                    raw_output = read_text(task_dir / "fallback.stdout.log")
        if result.status == "success":
            output_path = Path(result.stdout_path)
            worker_output = read_text(output_path)
            if self.is_unusable_executor_output(worker_output) or not self.has_valid_worker_result(worker_output):
                result = CommandResult(
                    name=result.name,
                    command=result.command,
                    cwd=result.cwd,
                    stdout_path=result.stdout_path,
                    stderr_path=result.stderr_path,
                    exit_code=result.exit_code,
                    status="failed",
                    message=f"{result.name} returned invalid worker output contract",
                )
        return TaskResult(
            task_id=task["task_id"],
            title=task["title"],
            executor=result.name,
            attempt=attempt,
            status=result.status,
            stdout_path=result.stdout_path,
            stderr_path=result.stderr_path,
            exit_code=result.exit_code,
            message=result.message,
        )

    def find_dotnet_solution(self) -> Path | None:
        solutions = sorted(self.repo_root.rglob("*.sln"))
        if solutions:
            return solutions[0]
        slnx = sorted(self.repo_root.rglob("*.slnx"))
        if slnx:
            return slnx[0]
        return None

    def has_dotnet_tests(self) -> bool:
        for project in self.repo_root.rglob("*.csproj"):
            lower = project.name.lower()
            if "test" in lower:
                return True
        return False

    def verification_commands(self) -> list[dict[str, Any]]:
        commands: list[dict[str, Any]] = []

        for project in self.repo_summary["node_projects"]:
            scripts = project.get("scripts", {})
            cwd = self.repo_root / project["path"]
            commands.append(
                {"name": f"lint:{project['path']}", "command": ["npm", "run", "lint"], "cwd": cwd}
                if "lint" in scripts
                else {"name": f"lint:{project['path']}", "command": None, "cwd": cwd, "reason": "No lint script found"}
            )
            commands.append(
                {"name": f"typecheck:{project['path']}", "command": ["npm", "run", "typecheck"], "cwd": cwd}
                if "typecheck" in scripts
                else (
                    {"name": f"typecheck:{project['path']}", "command": ["npx", "tsc", "--noEmit"], "cwd": cwd}
                    if (cwd / "tsconfig.json").exists()
                    else {"name": f"typecheck:{project['path']}", "command": None, "cwd": cwd, "reason": "No typecheck script or tsconfig.json found"}
                )
            )
            commands.append(
                {"name": f"test:{project['path']}", "command": ["npm", "run", "test"], "cwd": cwd}
                if "test" in scripts
                else {"name": f"test:{project['path']}", "command": None, "cwd": cwd, "reason": "No test script found"}
            )
            commands.append(
                {"name": f"build:{project['path']}", "command": ["npm", "run", "build"], "cwd": cwd}
                if "build" in scripts
                else {"name": f"build:{project['path']}", "command": None, "cwd": cwd, "reason": "No build script found"}
            )

        solution = self.find_dotnet_solution()
        if solution:
            rel = str(solution.relative_to(self.repo_root))
            commands.append({"name": f"lint:{rel}", "command": None, "cwd": solution.parent, "reason": "No .NET lint command configured; skipping gracefully"})
            commands.append({"name": f"typecheck:{rel}", "command": ["dotnet", "build", str(solution), "--nologo"], "cwd": solution.parent})
            commands.append(
                {"name": f"test:{rel}", "command": ["dotnet", "test", str(solution), "--no-build", "--nologo"], "cwd": solution.parent}
                if self.has_dotnet_tests()
                else {"name": f"test:{rel}", "command": None, "cwd": solution.parent, "reason": "No .NET test projects found"}
            )
            commands.append({"name": f"build:{rel}", "command": ["dotnet", "build", str(solution), "--nologo"], "cwd": solution.parent})

        for project_path in self.repo_summary["python_projects"]:
            cwd = self.repo_root / project_path
            commands.append({"name": f"lint:{project_path}", "command": ["ruff", "check", "."], "cwd": cwd})
            commands.append({"name": f"typecheck:{project_path}", "command": ["mypy", "."], "cwd": cwd})
            commands.append({"name": f"test:{project_path}", "command": ["pytest"], "cwd": cwd})
            commands.append({"name": f"build:{project_path}", "command": None, "cwd": cwd, "reason": "No generic Python build step"})

        return commands

    def run_command(self, name: str, command: list[str] | None, cwd: Path, output_dir: Path, reason: str | None = None) -> CommandResult:
        stdout_path = output_dir / "stdout.log"
        stderr_path = output_dir / "stderr.log"
        self.guard.assert_write_path(stdout_path)
        self.guard.assert_write_path(stderr_path)
        output_dir.mkdir(parents=True, exist_ok=True)

        if not command:
            write_text(stdout_path, "")
            write_text(stderr_path, reason or "")
            return CommandResult(name, [], str(cwd), str(stdout_path), str(stderr_path), None, "skipped", reason or "Command skipped")

        if self.mode == "dry-run":
            write_text(stdout_path, f"[dry-run] {' '.join(command)}\n")
            write_text(stderr_path, "")
            return CommandResult(name, command, str(cwd), str(stdout_path), str(stderr_path), 0, "dry-run", "Verification not executed because mode=dry-run")

        resolved = shutil.which(command[0]) or command[0]
        command = [resolved, *command[1:]]
        try:
            completed = subprocess.run(
                command,
                cwd=str(cwd),
                text=True,
                encoding="utf-8",
                errors="replace",
                capture_output=True,
                shell=False,
                timeout=self.verification_timeout_seconds,
            )
        except FileNotFoundError:
            write_text(stdout_path, "")
            write_text(stderr_path, f"Command not found: {command[0]}")
            return CommandResult(name, command, str(cwd), str(stdout_path), str(stderr_path), None, "failed", f"Command not found: {command[0]}")
        except subprocess.TimeoutExpired as exc:
            write_text(stdout_path, exc.stdout or "")
            write_text(stderr_path, exc.stderr or f"Verification timeout after {self.verification_timeout_seconds} seconds")
            return CommandResult(name, command, str(cwd), str(stdout_path), str(stderr_path), None, "timeout", f"{name} timed out")
        write_text(stdout_path, completed.stdout or "")
        write_text(stderr_path, completed.stderr or "")
        status = "success" if completed.returncode == 0 else "failed"
        return CommandResult(name, command, str(cwd), str(stdout_path), str(stderr_path), completed.returncode, status, f"{name} exited with code {completed.returncode}")

    def verify_phase(self, phase: dict[str, Any], attempt: int) -> list[CommandResult]:
        results: list[CommandResult] = []
        for command in self.verification_commands():
            results.append(
                self.run_command(
                    name=command["name"],
                    command=command.get("command"),
                    cwd=Path(command["cwd"]),
                    output_dir=self.log_dir / phase["phase_id"] / "verification" / f"attempt-{attempt}" / slugify(command["name"]),
                    reason=command.get("reason"),
                )
            )
        return results

    def repair_phase(self, phase: dict[str, Any], attempt: int, failures: list[CommandResult]) -> TaskResult:
        model = self.select_model(phase, {"title": phase["title"], "description": "repair verification failures"}, "repair")
        prompt = self.prompt_text(
            "verifier.md",
            textwrap.dedent(
                f"""
                Phase state summary:
                {json.dumps(self.load_phase_state_summary(), indent=2)}

                Model selection:
                {json.dumps(asdict(model), indent=2)}

                Repository root: {self.repo_root}
                Phase: {phase['phase_id']} - {phase['title']}
                Attempt: {attempt}

                Failed verification results:
                {json.dumps([asdict(item) for item in failures], indent=2)}

                Use the logs referenced above to repair the phase without expanding scope.
                """
            ),
        )
        repair_dir = self.log_dir / phase["phase_id"] / "repair" / f"attempt-{attempt}"
        result = self.invoke_executor("claude", prompt, None, repair_dir / "stdout.log", repair_dir / "stderr.log", self.repo_root, self.executor_timeout_seconds)
        return TaskResult(
            task_id=f"{phase['phase_id']}-repair-{attempt}",
            title=f"Repair verification failures for {phase['phase_id']}",
            executor="claude",
            attempt=attempt,
            status=result.status,
            stdout_path=result.stdout_path,
            stderr_path=result.stderr_path,
            exit_code=result.exit_code,
            message=result.message,
        )

    def write_phase_state(
        self,
        phase: dict[str, Any],
        final_status: str,
        attempt: int,
        task_results: list[TaskResult],
        verification_results: list[CommandResult],
    ) -> None:
        payload = {
            "phase_id": phase["phase_id"],
            "title": phase["title"],
            "status": final_status,
            "attempts_used": attempt,
            "max_retries": self.max_phase_retries,
            "timestamp": utc_now(),
            "task_results": [asdict(item) for item in task_results],
            "verification_results": [asdict(item) for item in verification_results],
        }
        phase_state_path = self.state_dir / f"{phase['phase_id']}.json"
        self.guard.assert_write_path(phase_state_path)
        write_json(phase_state_path, payload)

    def run(self) -> None:
        self.ensure_prerequisites()
        if not self.assessment_path.exists():
            self.fail(f"Assessment file not found: {self.assessment_path}")

        assessment_text = read_text(self.assessment_path)
        write_json(self.run_dir / "repo_summary.json", self.repo_summary)
        plan = self.create_plan(assessment_text)

        run_summary: dict[str, Any] = {
            "run_id": self.run_id,
            "assessment_file": str(self.assessment_path),
            "mode": self.mode,
            "max_phase_retries": self.max_phase_retries,
            "planner_executor": self.planner_executor,
            "timestamp": utc_now(),
            "phases": [],
        }
        completed_phases: set[str] = set()

        for phase in plan["phases"]:
            deps = set(phase.get("depends_on", []))
            if not deps.issubset(completed_phases):
                self.fail("Phase dependency order is invalid.", {"phase": phase, "completed": sorted(completed_phases)})

            all_task_results: list[TaskResult] = []
            final_verification_results: list[CommandResult] = []
            final_status = "failed"

            for attempt in range(1, self.max_phase_retries + 1):
                task_failure = False
                for task in self.topological_tasks(phase.get("tasks", [])):
                    result = self.run_phase_task(phase, task, attempt)
                    all_task_results.append(result)
                    if result.status == "failed":
                        task_failure = True
                        break

                if task_failure:
                    continue

                verification_results = self.verify_phase(phase, attempt)
                final_verification_results = verification_results
                failures = [item for item in verification_results if item.status == "failed"]
                if not failures:
                    final_status = "success" if self.mode == "execute" else "dry-run"
                    self.write_phase_state(phase, final_status, attempt, all_task_results, final_verification_results)
                    break

                all_task_results.append(self.repair_phase(phase, attempt, failures))
                if attempt == self.max_phase_retries:
                    self.write_phase_state(phase, final_status, attempt, all_task_results, final_verification_results)
                    self.fail(f"Phase {phase['phase_id']} failed verification after {self.max_phase_retries} attempts.", {"phase": phase["phase_id"]})
            else:
                self.write_phase_state(phase, final_status, self.max_phase_retries, all_task_results, final_verification_results)
                self.fail(f"Phase {phase['phase_id']} did not complete successfully.", {"phase": phase["phase_id"]})

            completed_phases.add(phase["phase_id"])
            run_summary["phases"].append({"phase_id": phase["phase_id"], "title": phase["title"], "status": final_status})
            write_json(self.run_dir / "run_summary.json", run_summary)

        run_summary["status"] = "success" if self.mode == "execute" else "dry-run"
        write_json(self.run_dir / "run_summary.json", run_summary)


def parse_args() -> argparse.Namespace:
    parser = argparse.ArgumentParser(description="Autonomous review improvement orchestrator")
    parser.add_argument("assessment_file", nargs="?", help="Path to the review/assessment file")
    parser.add_argument("--mode", choices=("dry-run", "execute"), default=None, help="Execution mode")
    parser.add_argument("--planner-executor", choices=("local", "claude", "gemini"), default=None, help="Executor used for phase planning")
    parser.add_argument("--max-phase-retries", type=int, default=None, help="Override retry count per phase")
    return parser.parse_args()


def main() -> int:
    args = parse_args()
    try:
        ReviewOrchestrator(args).run()
    except RuntimeError as exc:
        print(str(exc), file=sys.stderr)
        return 1
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
