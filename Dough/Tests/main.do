def main: void {
  loop(0, 1000)
}

def loop: void(start: i32, end: i32) {
  if start == end
    then return
    else 0

  if start % 2 == 0
    then 0
    else print start

  loop(start + 1, end)
}