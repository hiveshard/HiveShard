import { elementToSVG } from "dom-to-svg";

window.elementToSVGString = el =>
    new XMLSerializer().serializeToString(elementToSVG(el));