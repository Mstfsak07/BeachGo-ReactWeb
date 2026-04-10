import { spawn, execSync } from "child_process";
import net from "net";
import fs from "fs";
import path from "path";

const processes = {};
const restartCounts = {};
const MAX_RESTARTS = 5;
let agentStarted = false;
let isTestRunning = false;
let isShuttingDown = false;

const FRONTEND_DIR = path.join(process.cwd(), "beach-ui");
const backendDir = path.join(process.cwd(), "BeachRehberi.API", "BeachRehberi.API");
const AGENT_SCRIPT = path.join(process.cwd(), "automation", "scripts", "run-loop.ps1");
console.log("Backend dir:", backendDir);

function safeWrite(stream, text) {
  if (!stream || stream.destroyed || !stream.writable) return;
  try {
    stream.write(text);
  } catch (error) {
    if (!error || error.code !== "EPIPE") {
      console.error("[WRITE ERROR]", error);
    }
  }
}

function killProcessTree(pid) {
  try {
    if (process.platform === "win32") {
      execSync(`taskkill /PID ${pid} /T /F`, { stdio: "ignore" });
    } else {
      process.kill(-pid, "SIGKILL");
    }
  } catch {}
}

function gracefulShutdown(signal) {
  if (isShuttingDown) return;
  isShuttingDown = true;
  console.log(`\n[SHUTDOWN] ${signal} received. Killing all child processes...`);

  for (const [name, proc] of Object.entries(processes)) {
    console.log(`[SHUTDOWN] Killing ${name} (PID=${proc.pid})...`);
    killProcessTree(proc.pid);
  }

  try {
    if (process.platform === "win32") {
      execSync('taskkill /IM "gemini.exe" /F', { stdio: "ignore" });
    }
  } catch {}

  console.log("[SHUTDOWN] All processes killed. Exiting.");
  process.exit(0);
}

process.on("SIGINT", () => gracefulShutdown("SIGINT"));
process.on("SIGTERM", () => gracefulShutdown("SIGTERM"));
process.on("SIGHUP", () => gracefulShutdown("SIGHUP"));
if (process.platform === "win32") {
  process.on("message", (msg) => {
    if (msg === "shutdown") gracefulShutdown("shutdown");
  });
}

// 🔍 PORT KONTROL
function isRunning(port) {
  return new Promise((resolve) => {
    const socket = new net.Socket();

    socket.setTimeout(1000);

    socket
      .once("connect", () => {
        socket.destroy();
        resolve(true);
      })
      .once("timeout", () => {
        socket.destroy();
        resolve(false);
      })
      .once("error", () => {
        resolve(false);
      })
      .connect(port, "127.0.0.1");
  });
}

// ▶️ PROCESS BAŞLAT
function startProcess(name, cmd, args, cwd) {
  if (processes[name]) {
    console.log(`[SKIP] ${name} already running`);
    return;
  }

  console.log(`[START] ${name}`);

  const fullCmd = [cmd, ...args].join(" ");
  const proc = spawn(fullCmd, [], {
    cwd,
    shell: true,
    env: process.env,
  });

  processes[name] = proc;

  proc.stdout.on("data", (data) => {
    safeWrite(process.stdout, `[${name}] ${data}`);
  });

  proc.stderr.on("data", (data) => {
    safeWrite(process.stderr, `[${name} ERROR] ${data}`);
  });

  proc.stdout.on("error", (error) => {
    if (error.code !== "EPIPE") {
      console.error(`[${name} STDOUT ERROR]`, error);
    }
  });

  proc.stderr.on("error", (error) => {
    if (error.code !== "EPIPE") {
      console.error(`[${name} STDERR ERROR]`, error);
    }
  });

  proc.on("close", (code) => {
    console.log(`[EXIT] ${name} (${code})`);
    delete processes[name];

    if (name === "AGENT") {
      console.log(`[AGENT] Loop surecini tamamladi (code=${code}). Yeniden baslatilmayacak.`);
      return;
    }

    if (isShuttingDown) return;

    restartCounts[name] = (restartCounts[name] || 0) + 1;
    if (restartCounts[name] > MAX_RESTARTS) {
      console.log(`[STOP] ${name} reached max restart limit (${MAX_RESTARTS}). Giving up.`);
      return;
    }

    const delay = Math.min(5000 * restartCounts[name], 30000);
    setTimeout(() => {
      console.log(`[RESTART] ${name} (attempt ${restartCounts[name]}/${MAX_RESTARTS})`);
      startProcess(name, cmd, args, cwd);
    }, delay);
  });
}

// 🚀 BAŞLATMA KONTROLÜ
async function startAll() {
  // BACKEND
  if (!(await isRunning(5144))) {
    startProcess(
      "Backend",
      "dotnet",
      ["run", "--project", "BeachRehberi.API.csproj"],
      backendDir
    );
  } else {
    console.log("[OK] Backend already running");
  }

  // FRONTEND
  if (!(await isRunning(3000))) {
    startProcess(
      "FRONTEND",
      "npm",
      ["start"],
      FRONTEND_DIR
    );
  } else {
    console.log("[OK] Frontend already running");
  }

  // AGENT (tek instance)
  if (!agentStarted) {
    if (!fs.existsSync(AGENT_SCRIPT)) {
      console.log(`[ERROR] Agent script not found: ${AGENT_SCRIPT}`);
    } else {
      agentStarted = true;

      startProcess(
        "AGENT",
        "powershell",
        [
          "-ExecutionPolicy",
          "Bypass",
          "-File",
          AGENT_SCRIPT,
        ],
        process.cwd()
      );
    }
  }
}

// 🧪 TEST SİSTEMİ
async function runTests() {
  console.log("\n=== TEST START ===");

  const run = (cmd, cwd) =>
    new Promise((resolve) => {
      const p = spawn(cmd, [], { shell: true, cwd });

      let output = "";

      p.stdout.on("data", (d) => (output += d));
      p.stderr.on("data", (d) => (output += d));

      p.on("close", (code) => resolve({ output, code }));
    });

  const backend = await run("dotnet build", "./BeachRehberi.API");
  const frontend = await run("npm run build", "./beach-ui");

  const result = `BACKEND (exit ${backend.code}):\n${backend.output}\n\nFRONTEND (exit ${frontend.code}):\n${frontend.output}`;

  fs.writeFileSync("test-result.txt", result);

  if (backend.code !== 0 || frontend.code !== 0) {
    console.log("[TEST] FAILED - check test-result.txt for details");
  }

  console.log("=== TEST DONE ===\n");
}

// 🔔 TEST TETİKLEYİCİ
setInterval(async () => {
  if (isTestRunning) return;
  if (fs.existsSync("run-tests.txt")) {
    fs.unlinkSync("run-tests.txt");
    isTestRunning = true;
    try {
      await runTests();
    } finally {
      isTestRunning = false;
    }
  }
}, 3000);

// 🚀 BAŞLAT
startAll();
