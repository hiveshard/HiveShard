#!/usr/bin/env bash

MIN=-4
MAX=4

echo '<div id="diagram">'

for y in $(seq "$MIN" "$MAX"); do
  for x in $(seq "$MIN" "$MAX"); do
    cat <<EOF
    <div class="cell" id="($x,$y)"><div class="title">($x,$y)</div><div class="bars"><span>A</span><span>B</span></div></div>
EOF
  done
done

echo '</div>'