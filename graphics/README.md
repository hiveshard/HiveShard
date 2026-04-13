# html diagram graphics to portable svg converter

## Requirements

- node (v22.9.0 worked)
- chromium

## Getting started

Install dependencies from npm:
```bash
cd ./graphics
npm install
```

Run with node
```bash
PUPPETEER_EXECUTABLE_PATH=$(which chromium) node render.js shardgrid.html > shardgrid.svg
```