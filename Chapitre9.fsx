﻿// Programmation Asynchrone et Parallèle
open System
open System.Threading
open System.Collections.Generic
open System.IO

// THREADS
// Création et lancement de threads 
let corpsDuThread () =
    for i in 1..5 do
        // pause d'un dixième de seconde
        Thread.Sleep(100)
        printfn "[Thread %d] -> %d" Thread.CurrentThread.ManagedThreadId i

let lanceThread () =
    let thread = new Thread(corpsDuThread)
    thread.Start ()

lanceThread ()
lanceThread ()

// Pool de threads

// on passe une fonction lambda sans avoir besoin d'instancier le délégué WaitCallback
ThreadPool.QueueUserWorkItem(fun _ -> for i in 1..5 do printfn "%d" i)

//soit un crée une instance de WaitCallback
let afficheNombre (max : obj) =
    for i in 1 .. (max :?> int) do
        printfn "%d" i
// soit on utilise explicitement le délégué avec la nécessité de faire un boxing sur le type int
ThreadPool.QueueUserWorkItem(new WaitCallback(afficheNombre), box 5)

// Problèmes : compétitions / race conditions
let sommeTableauErreur (tab: int[]) = 
    let total = ref 0

    let thread1Achevé = ref false

    ThreadPool.QueueUserWorkItem(
        fun _ -> for i = 0 to tab.Length / 2 - 1 do
                    total := tab.[i] + !total
                 thread1Achevé := true
    ) |> ignore

    let thread2Achevé = ref false

    ThreadPool.QueueUserWorkItem(
        fun _ -> for i = tab.Length  / 2 to tab.Length - 1 do
                    total := tab.[i] + !total
                 thread2Achevé := true
    ) |> ignore

    while !thread1Achevé = false || !thread2Achevé = false do
        Thread.Sleep(0)

    !total

let millionUn = Array.create 1000000 1

sommeTableauErreur millionUn

// résolution sans blocage: 

let sommeTableauNonBloquée (tab:int[]) =
    let sstotal1 = ref 0
    let sstotal2 = ref 0
    let thread1Achevé = ref false

    ThreadPool.QueueUserWorkItem(
        fun _ -> for i = 0 to tab.Length / 2 - 1 do
                    sstotal1 := tab.[i] + !sstotal1
                 thread1Achevé := true
    ) |> ignore

    let thread2Achevé = ref false

    ThreadPool.QueueUserWorkItem(
        fun _ -> for i = tab.Length  / 2 to tab.Length - 1 do
                    sstotal2 := tab.[i] + !sstotal2
                 thread2Achevé := true
    ) |> ignore

    // attendre que les 2 threads aient fini leur travail respectif
    while !thread1Achevé = false || !thread2Achevé = false do
        Thread.Sleep(0)

    !sstotal1 + !sstotal2

sommeTableauNonBloquée millionUn
sommeTableauNonBloquée millionUn

// résolution avec la fonction lock : ('a -> (unit -> 'b) -> 'b) when 'a : not struct

let sommeTableauBloqué (tab:int[]) =
    let total = ref 0

    let thread1Achevé = ref false
    ThreadPool.QueueUserWorkItem(
        fun _ -> for i = 0 to tab.Length / 2 - 1 do
                    lock total (fun () -> total := tab.[i] + !total)
                 thread1Achevé := true
    ) |> ignore

    let thread2Achevé = ref false
    ThreadPool.QueueUserWorkItem(
        fun _ -> for i = tab.Length / 2 to tab.Length - 1 do
                    lock total (fun () -> total := tab.[i] + !total)
                 thread2Achevé := true
    ) |> ignore

    while !thread1Achevé = false || !thread2Achevé = false do
        Thread.Sleep(0)

    !total

sommeTableauBloqué millionUn
