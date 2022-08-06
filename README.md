# Dough

## Syntax Example
```
def main: void() {
  let x = 10
  let y: i32 = 20
  let z = x + y
  
  abs(-50)
  pow(5, 3)
}

def abs: void(val: i32) {
  return if val < 0
    then 0 - val
    else val
}

def pow(x: i32, y: i32) {
  if y == 1
    then return x
    else return x * pow(x, y - 1)
}
```
