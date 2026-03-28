const fs = require("fs");
const path = require("path");

function fix(text) {
    try {
        return Buffer.from(text, "latin1").toString("utf8");
    } catch {
        return text;
    }
}

function processFile(file) {
    let content = fs.readFileSync(file, "utf8");
    let fixed = fix(content);

    if (content !== fixed) {
        fs.writeFileSync(file, fixed, "utf8");
        console.log("FIXED:", file);
    }
}

function walk(dir) {
    fs.readdirSync(dir).forEach(file => {
        const full = path.join(dir, file);

        if (fs.statSync(full).isDirectory()) {
            walk(full);
        } else if (/\.(js|jsx|ts|tsx|json|html|css)$/.test(file)) {
            processFile(full);
        }
    });
}

walk("./");