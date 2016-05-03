﻿open System

//null et les types référence
let estNull = 
    function 
    | null -> true
    | _ -> false

estNull "une chaîne"
estNull (null : string)

// Alias et types référence
let x = [| 0 |]
let y = x

x
y
x.[0] <- 3 // cf. infra
y

// mutabilité des variables
let mutable message = "le monde"

printfn "Salut %s !" message
message <- "l'Univers"
printfn "Salut %s !" message

// restrictions avec la mutabilité pour
// les versions antérieures de F# 4.0
let utilisationInvalide() = 
    let mutable x = 0
    // avec Monodevelop : on a une erreur de l'analyseur 
    // en revanche il est parfaitement géré par Ionide
    let incrementX() = x <- x + 1
    incrementX()
    x

// ce code est parfaitement compris et géré par F#4
utilisationInvalide()

// les références
let planetes = ref [ "Mercure"; "Vénus"; "Terre"; "Mars"; "Jupiter"; "Saturne"; "Uranus"; "Neptune"; "Pluton" ]

// Pluton ne faisant plus partie de la liste des planètes
// on récupère les valeurs de planètes via !
// et on réaffecte la nouvelle liste à planetes via :=
planetes := !planetes |> List.filter (fun p -> p <> "Pluton")
!planetes

// incrémentation et décrémentation des références entières
let z = ref 0

printfn "Le contenu de la référence z : %d" z.contents
incr z
printfn "Le nouveau contenu de la référence z : %d" z.contents
decr z
printfn "Le dernier contenu de la référence z : %d" z.contents

// RECORDS MUTABLES
type VoitureMutable = 
    { Constructeur : string
      Modele : string
      mutable Km : int }

let kilometrage voiture = 
    let alea = new Random()
    voiture.Km <- voiture.Km + alea.Next() % 100

let mustang = 
    { Constructeur = "Ford"
      Modele = "Mustang"
      Km = 0 }

kilometrage mustang
kilometrage mustang
kilometrage mustang
mustang.Km

// UNITES DE MESURE
[<Measure>]
type uniteDeMesure

1.0<uniteDeMesure> * 1.0<uniteDeMesure>

[<Measure>]
type metre

[<Measure>]
type seconde

1.0<metre> / 1.0<metre>
1.0<metre> / 1.0<metre> / 1.0<metre>

// 12 mètres par secondes
let douze = 12<metre> / 1<seconde>

// Définir ses propres unités en les "reliant"
// comme on a déjà l'unité seconde : on va la réutiliser
[<Measure>]
type Hz = seconde^-1

3.0<seconde ^-1> = 3.0<Hz>

// exemple plus complexe avec l'ajout de méthodes statiques en parallélisant les unités
[<Measure>]
type far = 
    static member ConvertirEnCelsius(x : float<far>) = (5.0<cel> / 9.0<far>) * (x - 32.0<far>)

and [<Measure>] cel = 
    static member ConvertirEnFahrenheit(x : float<cel>) = (9.0<far> / 5.0<cel> * x) + 32.0<far>

far.ConvertirEnCelsius 20.0<far>

let afficheTemperature (tmp : float<far>) = 
    match tmp with
    | _ when tmp < 32.0<_> -> printfn "Ca gèle"
    | _ when tmp < 65.0<_> -> printfn "Froid"
    | _ when tmp < 75.0<_> -> printfn "Parfait !"
    | _ when tmp < 100.0<_> -> printfn "Chaud"
    | _ -> printfn "Brûlant"

open Microsoft.FSharp.Data.UnitSystems.SI.UnitSymbols

let flash = 8.0<cd> // candela

open Microsoft.FSharp.Data.UnitSystems.SI.UnitNames

let intensite = 32.0<ohm>
// unités de mesure générique
let carreGenerique (x : float<_>) = x * x

carreGenerique 2.0
carreGenerique 2.0<metre>

// Objet et unités de mesure génériques
type Point<[<Measure>] 'u>(x : float<'u>, y : float<'u>) = 
    member this.X = x
    member this.Y = y
    member this.XSansUnite = (float this.X)
    member this.YSansUnite = (float this.Y)
    member this.Length = x * y
    override this.ToString() = sprintf "(%f, %f)" this.XSansUnite this.YSansUnite

let point = new Point<metre>(3.0<metre>, 2.0<metre>)

point.XSansUnite
point.YSansUnite
point.Length
point.ToString()

// TABLEAUX
// façon compréhension
let carres = 
    [| for i in 1..7 -> i * i |]

// façon traditionnelle
let carres2 = [| 1; 4; 9; 16; 25; 36; 49; 64; 81; 100 |]

// Utilisation du chiffre ROT13 
let ROT13(texte : string) = 
    let tabCaracteres = Array.ofSeq texte
    
    let rot13 (caractere : char) = 
        if Char.IsLetter caractere then 
            let nvCaractere = 
                (int caractere)
                |> (fun x -> x - (int 'A'))
                |> (fun x -> (x + 13) % 26)
                |> (fun x -> x + (int 'A'))
            (char nvCaractere)
        else caractere
    for i in 0..(tabCaracteres.Length - 1) do
        tabCaracteres.[i] <- rot13 tabCaracteres.[i]
    Array.foldBack (fun element str -> sprintf "%c%s" element str) tabCaracteres ""

let chiffrement = ROT13 "PORTEZ CE VIEUX WHISKY AU JUGE BLOND QUI FUME"

printfn "CHIFFRE : %s" chiffrement
printfn "DECHIFFRE : %s" (ROT13 chiffrement)