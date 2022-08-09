# Dough
A functional language written in C# with Python & TypeScript inspired syntax, and a built in package manager (Oven).

Still a heavy WIP!

## Running
### Requirements

- .NET 6.0
- Visual Studio 2022 (reccomended)

## Instructions
- Clone the repo
- Run the below;
-     dotnet run Dough/Program.cs (flags)
 
## Flags
help                 - Prints a help message

create (packageName) - creates a new package with Oven

import (packageName) - imports a package

run (fileName)       - runs a file using .NET bytecode

build (fileName)     - builds a file to a bridge file

## Example program
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
