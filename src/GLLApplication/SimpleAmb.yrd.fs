
# 2 "SimpleAmb.yrd.fs"
module GLL.SimpleAmb
#nowarn "64";; // From fsyacc: turn off warnings that type variables used in production annotations are instantiated to concrete type
open Yard.Generators.GLL.Parser
open Yard.Generators.GLL
open Yard.Generators.Common.ASTGLL
type Token =
    | B of (int)
    | RNGLR_EOF of (int)


let genLiteral (str : string) (data : int) =
    match str.ToLower() with
    | x -> None
let tokenData = function
    | B x -> box x
    | RNGLR_EOF x -> box x

let numToString = function
    | 0 -> "error"
    | 1 -> "s"
    | 2 -> "yard_start_rule"
    | 3 -> "B"
    | 4 -> "RNGLR_EOF"
    | _ -> ""

let tokenToNumber = function
    | B _ -> 3
    | RNGLR_EOF _ -> 4

let isLiteral = function
    | B _ -> false
    | RNGLR_EOF _ -> false

let isTerminal = function
    | B _ -> true
    | RNGLR_EOF _ -> true
    | _ -> false

let numIsTerminal = function
    | 3 -> true
    | 4 -> true
    | _ -> false

let numIsNonTerminal = function
    | 0 -> true
    | 1 -> true
    | 2 -> true
    | _ -> false

let numIsLiteral = function
    | _ -> false

let getLiteralNames = []
let mutable private cur = 0

let acceptEmptyInput = false

let leftSide = [|1; 1; 1; 2|]
let table = [| [||];[||];[|2; 1; 0|];[||];[|3|];[||]; |]
let private rules = [|1; 1; 1; 1; 1; 3; 1; 4|]
let private canInferEpsilon = [|true; false; false; false; false|]
let defaultAstToDot =
    (fun (tree : Yard.Generators.Common.ASTGLL.Tree<Token>) -> tree.AstToDot numToString)

let private rulesStart = [|0; 3; 5; 6; 8|]
let startRule = 3
let indexatorFullCount = 5
let rulesCount = 4
let indexEOF = 4
let nonTermCount = 3
let termCount = 2
let termStart = 3
let termEnd = 4
let literalStart = 5
let literalEnd = 4
let literalsCount = 0

let slots = dict <| [|(-1, 0); (1, 1); (2, 2); (3, 3); (65537, 4); (65538, 5); (196609, 6)|]

let private parserSource = new ParserSourceGLL<Token> (Token.RNGLR_EOF 0, tokenToNumber, genLiteral, numToString, tokenData, isLiteral, isTerminal, getLiteralNames, table, rules, rulesStart, leftSide, startRule, literalEnd, literalStart, termEnd, termStart, termCount, nonTermCount, literalsCount, indexEOF, rulesCount, indexatorFullCount, acceptEmptyInput,numIsTerminal, numIsNonTerminal, numIsLiteral, canInferEpsilon, slots)
let buildAst : (seq<Token> -> ParseResult<_>) =
    buildAst<Token> parserSource


