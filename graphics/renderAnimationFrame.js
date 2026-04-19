import puppeteer from "puppeteer";
import path from "node:path";
import { pathToFileURL } from "node:url";

const input = process.argv[2];
const tick = process.argv[3];
const step = process.argv[4];
const frame = process.argv[5];
if (!input) throw new Error("usage: node renderAnimationFrame.js input.html TICK STEP FRAME");

const browser = await puppeteer.launch({
    executablePath: process.env.PUPPETEER_EXECUTABLE_PATH || "chromium",
    headless: true
});

const page = await browser.newPage();

await page.evaluateOnNewDocument((tick, step) => {
    window.tick = Number(tick);
    window.step = Number(step);
}, tick, step);

await page.goto(pathToFileURL(path.resolve(input)).href);
await page.addScriptTag({ path: path.resolve("inject.bundle.js") });

await page.waitForFunction(() => {
    const el = document.querySelector("#diagram");
    return el && el.dataset.rendered === "true";
});

await page.setViewport({
    width: 1920,
    height: 1080,
    deviceScaleFactor: 1
});

await page.screenshot({
    path: `../media/animation/frame_${frame}.png`,
    clip: {
        x: 0,
        y: 0,
        width: 1920,
        height: 1080
    }
});

await browser.close();