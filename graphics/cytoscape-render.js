import puppeteer from "puppeteer";
import path from "node:path";
import { pathToFileURL } from "node:url";

const input = process.argv[2];
if (!input) {
    console.error("usage: node render.js <input.html> > output.svg");
    process.exit(1);
}

const fileUrl = pathToFileURL(path.resolve(input)).href;

const browser = await puppeteer.launch({
    executablePath: process.env.PUPPETEER_EXECUTABLE_PATH || "chromium",
    headless: true
});

const page = await browser.newPage();

await page.goto(fileUrl, { waitUntil: 'networkidle0' });

// wait for cytoscape layout to finish
await page.waitForFunction(() => window.layoutDone === true, { timeout: 60000 });

// extract svg
const svg = await page.evaluate(() => cy.svg({ full: true }));

console.log(svg);

await browser.close();