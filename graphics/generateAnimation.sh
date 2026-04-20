#!/usr/bin/env bash

MAX_TICK=$1
MAX_STEP=$2

if [[ -z "$MAX_TICK" || -z "$MAX_STEP" ]]; then
  echo "usage: ./run.sh <max_tick> <max_step>"
  exit 1
fi

rm -f ../media/animation/frame_*
rm -f ../media/animation/*.mp4
rm -f temp/graph_*

FRAME=0
for ((t=0; t<MAX_TICK; t++)); do
  for ((s=0; s<MAX_STEP; s++)); do
    printf -v PAD "%03d" $FRAME
    echo "Rendering tick=$t step=$s → frame=$PAD"
    PUPPETEER_EXECUTABLE_PATH=$(which chromium) \
    node renderAnimationFrame.js "$t" "$s" "$PAD"
    ((FRAME++))
  done
done

ffmpeg -framerate 8 -i ../media/animation/frame_%03d.png \
  -c:v libx264 -pix_fmt yuv420p ../media/animation/out.mp4
  
ffmpeg -framerate 8 -i ../media/animation/frame_%03d.png \
  -vf "fps=8,scale=960:-1:flags=lanczos,palettegen" \
  ./temp/palette.png
  
ffmpeg -framerate 8 -i ../media/animation/frame_%03d.png \
  -i ./temp/palette.png \
  -loop 0 \
  -lavfi "fps=8,scale=960:-1:flags=lanczos[x];[x][1:v]paletteuse=dither=bayer" \
  ../media/animation/out.gif