#!/usr/bin/env bash

MAX_TICK=$1
MAX_STEP=$2

if [[ -z "$MAX_TICK" || -z "$MAX_STEP" ]]; then
  echo "usage: ./run.sh <max_tick> <max_step>"
  exit 1
fi

rm -f ../media/animation/frame_*

FRAME=0
for ((t=0; t<MAX_TICK; t++)); do
  for ((s=0; s<MAX_STEP; s++)); do
    printf -v PAD "%03d" $FRAME
    echo "Rendering tick=$t step=$s → frame=$PAD"
    PUPPETEER_EXECUTABLE_PATH=$(which chromium) \
    node renderAnimationFrame.js headline-animation-canvas.html "$t" "$s" "$PAD"
    ((FRAME++))
  done
done

ffmpeg -framerate 8 -i ../media/animation/frame_%03d.png \
  -c:v libx264 -pix_fmt yuv420p ../media/animation/out.mp4