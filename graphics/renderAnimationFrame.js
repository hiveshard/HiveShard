import puppeteer from "puppeteer";
import path from "node:path";
import fs from "node:fs/promises";
import { pathToFileURL } from "node:url";

const tick = process.argv[2];
const step = process.argv[3];
const frame = process.argv[4];
if (process.argv.length < 4) throw new Error("usage: node renderAnimationFrame.js TICK STEP FRAME");

const browser = await puppeteer.launch({
    executablePath: process.env.PUPPETEER_EXECUTABLE_PATH || "chromium",
    headless: true
});

// render cycle frame
const cycleFileUrl = pathToFileURL(path.resolve("headline-animated-cycles.html")).href;

const pageCycle = await browser.newPage();

await pageCycle.evaluateOnNewDocument((tick, step, frame) => {
    window.tick = Number(tick);
    window.step = Number(step);
    window.frame = Number(frame);
}, tick, step, frame);


await pageCycle.goto(cycleFileUrl, { waitUntil: 'networkidle0' });

await pageCycle.waitForFunction(() => window.layoutDone === true, { timeout: 60000 });

const svg = await pageCycle.evaluate(() => cy.svg({ full: true }));

const imgLocation = `./temp/graph_${frame}.svg`;
await fs.writeFile(imgLocation, svg);



// render frame
const pageFrame = await browser.newPage();

pageFrame.on("console", msg => {
    console.log("PAGE LOG:", msg.type(), msg.text());
});

pageFrame.on("pageerror", err => {
    console.error("PAGE ERROR:", err);
});

pageFrame.on("requestfailed", req => {
    console.error("REQUEST FAILED:", req.url(), req.failure()?.errorText);
});

const imgPath = pathToFileURL(
    path.resolve(imgLocation)
).href;

let attachGraphDynamically = 1; 

await pageFrame.evaluateOnNewDocument((tick, step, frame, imgPath, attachGraphDynamically) => {
    window.tick = Number(tick);
    window.step = Number(step);
    window.frame = Number(frame);
    window.imgPath = imgPath;
    window.attachGraphDynamically = Number(attachGraphDynamically);
}, tick, step, frame, imgPath, attachGraphDynamically);

await pageFrame.goto(pathToFileURL(path.resolve("headline-animation-canvas.html")).href);
await pageFrame.addScriptTag({ path: path.resolve("inject.bundle.js") });

await pageFrame.waitForFunction(() => {
    const el = document.querySelector("#diagram");
    return el && el.dataset.imageRendered === "true" && el.dataset.jsRan === "true";
});

await pageFrame.setViewport({
    width: 1920,
    height: 1080,
    deviceScaleFactor: 1
});

await pageFrame.screenshot({
    path: `../media/animation/frame_${frame}.png`,
    clip: {
        x: 0,
        y: 0,
        width: 1920,
        height: 1080
    }
});

await browser.close();