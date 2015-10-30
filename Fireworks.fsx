#if INTERACTIVE
#r "CursesSharp.dll"
#endif

open CursesSharp
open System

Curses.InitScr()

let [<Literal>] DelaySize = 20

let colorTable =
    [| Colors.RED; Colors.BLUE; Colors.GREEN; Colors.CYAN
       Colors.RED; Colors.MAGENTA; Colors.YELLOW; Colors.WHITE |]

let rng = Random()

let addStr (y, x) str =
    if (x >= 0 && x < Curses.Cols && y >= 0 && y < Curses.Lines) then
        Stdscr.Add(y, x, (str: string))

let refreshWithDelay delay =
    Curses.NapMs(delay)
    Stdscr.Move(Curses.Lines - 1, Curses.Cols - 1)
    Stdscr.Refresh()

let refresh () = refreshWithDelay DelaySize

let getColor () =
    let bold = if rng.Next(2) > 0 then Attrs.BOLD else Attrs.NORMAL
    Stdscr.Attr <- Curses.COLOR_PAIR(rng.Next(8)) ||| bold

let explode (row, col) =
    Stdscr.Erase()
    addStr (row, col) "-"
    refresh ()

    let col = col - 1
    getColor ()
    addStr (row - 1, col) " - "
    addStr (row,     col) "-+-"
    addStr (row + 1, col) " - "

    let col = col - 1
    getColor ()
    addStr (row - 2, col) " --- "
    addStr (row - 1, col) "-+++-"
    addStr (row,     col) "-+#+-"
    addStr (row + 1, col) "-+++-"
    addStr (row + 2, col) " --- "
    refreshWithDelay (DelaySize * 4)

    getColor ()
    addStr (row - 2, col) " +++ "
    addStr (row - 1, col) "++#++"
    addStr (row,     col) "+# #+"
    addStr (row + 1, col) "++#++"
    addStr (row + 2, col) " +++ "
    refreshWithDelay (DelaySize * 4)

    getColor ()
    addStr (row - 2, col) "  #  "
    addStr (row - 1, col) "## ##"
    addStr (row,     col) "#   #"
    addStr (row + 1, col) "## ##"
    addStr (row + 2, col) "  #  "
    refreshWithDelay (DelaySize * 4)

    getColor ()
    addStr (row - 2, col) " # # "
    addStr (row - 1, col) "#   #"
    addStr (row,     col) "     "
    addStr (row + 1, col) "#   #"
    addStr (row + 2, col) " # # "
    refreshWithDelay (DelaySize * 4)

try

    Stdscr.Blocking <- false
    Curses.Echo <- false

    if (Curses.HasColors) then
        Curses.StartColor()
        colorTable
        |> Array.iteri (fun i c ->
            Curses.InitPair(int16 i + 1s, c, Colors.BLACK))

    let flag = ref 0
    while (Stdscr.GetChar() = -1) do
        let rec loop () =
            let start = rng.Next(Curses.Cols - 3)
            let end' = rng.Next(Curses.Cols - 3)
            let start = if start < 2 then 2 else start
            let end' = if end' < 2 then 2 else end'
            let direction = if start > end' then -1 else 1
            let diff = abs (start - end')
            if diff < 2 || diff >= Curses.Lines - 2 then
                loop ()
            else
                start, diff, direction
        let start, diff, direction = loop ()

        Stdscr.Attr <- Attrs.NORMAL
        let row = ref 0
        for row' = 1 to diff - 1 do
            row := row'
            Stdscr.Add(Curses.Lines - row', row' * direction + start, if direction < 0 then "\\" else "/")
            if !flag > 0 then
                refresh()
                Stdscr.Erase()
                flag := 0
            incr flag

        if !flag > 0 then
            refresh()
            flag := 0
        incr flag

        explode (Curses.Lines - !row, diff * direction + start)
        Stdscr.Erase()
        refresh()

finally
    Curses.EndWin()
