import { spawn } from "child_process";
import net from "net";
import fs from "fs";

const processes = {};

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

  const proc = spawn(cmd, args, {
    cwd,
    shell: true,
    env: process.env,
  });

  processes[name] = proc;

  proc.stdout.on("data", (data) => {
    process.stdout.write(`[${name}] ${data}`);
  });

  proc.stderr.on("data", (data) => {
    process.stderr.write(`[${name} ERROR] ${data}`);
  });

  proc.on("exit", (code) => {
    console.log(`[EXIT] ${name} (${code})`);
    delete processes[name];

    // 🔁 Crash restart (delay ile)
    setTimeout(() => {
      console.log(`[RESTART] ${name}`);
      startAll();
    }, 5000);
  });
}

// 🚀 BAŞLATMA KONTROLÜ
async function startAll() {
  // BACKEND
  if (!(await isRunning(5144))) {
    startProcess(
      "BACKEND",
      "dotnet",
      ["run"],
      "./BeachRehberi.API"
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
      "./beach-ui"
    );
  } else {
    console.log("[OK] Frontend already running");
  }

  // AGENT (tek instance)
  if (!processes["AGENT"]) {
    startProcess(
      "AGENT",
      "powershell",
      [
        "-NonInteractive",
        "-ExecutionPolicy",
        "Bypass",
        "-File",
        "automation/scripts/run-loop.ps1",
      ],
      "."
    );
  }
}

// 🧪 TEST SİSTEMİ
async function runTests() {
  console.log("\n=== TEST START ===");

  const run = (cmd, cwd) =>
    new Promise((resolve) => {
      const p = spawn(cmd, { shell: true, cwd });

      let output = "";

      p.stdout.on("data", (d) => (output += d));
      p.stderr.on("data", (d) => (output += d));

      p.on("close", () => resolve(output));
    });

  const backend = await run("dotnet build", "./BeachRehberi.API");
  const frontend = await run("npm run build", "./beach-ui");

  const result = `BACKEND:\n${backend}\n\nFRONTEND:\n${frontend}`;

  fs.writeFileSync("test-result.txt", result);

  console.log("=== TEST DONE ===\n");
}

// 🔔 TEST TETİKLEYİCİ
setInterval(async () => {
  if (fs.existsSync("run-tests.txt")) {
    fs.unlinkSync("run-tests.txt");
    await runTests();
  }
}, 3000);

// 🚀 BAŞLAT
startAll();