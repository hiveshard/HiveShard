import puppeteer from "puppeteer";
import path from "node:path";
import { pathToFileURL } from "node:url";

const input = process.argv[2];
if (!input) throw new Error("usage: node render.js input.html > output.svg");

const browser = await puppeteer.launch({
    executablePath: process.env.PUPPETEER_EXECUTABLE_PATH || "chromium",
    headless: true
});

const page = await browser.newPage();

await page.goto(pathToFileURL(path.resolve(input)).href);
await page.addScriptTag({ path: path.resolve("inject.bundle.js") });

const svg = await page.$eval("#diagram", el => window.elementToSVGString(el));

console.log(svg);

await browser.close();