extern GetStdHandle: i32(stdHandle: i32) from "kernel32.dll"
extern WriteConsoleW: i32(handle: i32, buffer: string, length: i32, written: i32, reserved: i32) from "kernel32.dll"

extern MessageBoxW: i32(handle: i32, text: string, caption: string, type: i32) from "user32.dll"

def print: void(s: string, len: i32) {
  let handle = GetStdHandle(0 - 11)
  WriteConsoleW(handle, s, len, 0, 0)
}

def main: void() {
  print("hello world", 11)

  MessageBoxW(0, "hello world", "this is a title", 0)
}