const ts = require('typescript');
const path = require('path');
const fs = require('fs');

function findConfig() {
  const candidates = ['tsconfig.app.json', 'tsconfig.json'];
  for (const c of candidates) {
    const p = path.resolve(process.cwd(), c);
    if (fs.existsSync(p)) return p;
  }
  throw new Error('No tsconfig found (looked for tsconfig.app.json and tsconfig.json)');
}

try {
  const configPath = findConfig();
  const configFile = ts.readConfigFile(configPath, ts.sys.readFile);
  const parsed = ts.parseJsonConfigFileContent(configFile.config, ts.sys, path.dirname(configPath));
  const program = ts.createProgram(parsed.fileNames, parsed.options);

  let scanned = 0;
  let problems = 0;

  for (const sourceFile of program.getSourceFiles()) {
    scanned++;
    const arr = sourceFile.referencedFiles;
    if (arr && arr.length) {
      arr.forEach((entry, idx) => {
        if (!entry || typeof entry.pos === 'undefined' || typeof entry.end === 'undefined') {
          problems++;
          console.log('=== BAD referencedFiles ENTRY ===');
          console.log('file:', sourceFile.fileName);
          console.log('index:', idx);
          console.log('entry:', entry);
          console.log('----');
        }
      });
    }
  }

  console.log(`scanned files: ${scanned}`);
  console.log(`bad entries found: ${problems}`);
  if (problems === 0) console.log('No malformed referencedFiles entries detected.');
} catch (err) {
  console.error('Error running diagnostic:', err && err.stack ? err.stack : err);
  process.exit(2);
}
