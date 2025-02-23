﻿module YC.TSQL.Test

open NUnit.Framework
open Microsoft.FSharp.Collections
open QuickGraph
open AbstractAnalysis.Common
open YC.FST.GraphBasedFst
open YC.FST.AbstractLexing.Interpreter
open YC.FST.AbstractLexing.Tests.CommonTestChecker
open YC.FSA.GraphBasedFsa
open YC.FSA.FsaApproximation
open Yard.Examples.MSParser
open Graphviz4Net.Dot.AntlrParser
open Graphviz4Net.Dot
open Yard.Generators.RNGLR.AbstractParser
open Yard.Generators.ARNGLR.Parser
open RNGLRAbstractParserTests
open System

let baseInputGraphsPath = "../../../src/TSQL.Test/DotTSQL"

let transform x = (x, match x with |Smbl(y:char, _) when y <> (char 65535) -> Smbl(int <| Convert.ToUInt32(y)) |Smbl(y:char, _) when y = (char 65535) -> Smbl 65535 |_ -> Eps)
let smblEOF = Smbl(char 65535,  Unchecked.defaultof<Position<_>>)
let equalSmbl x y = (fst x) = (fst y)
let newSmb x =  Smbl(x, Unchecked.defaultof<_>)
let path baseInputGraphsPath name = System.IO.Path.Combine(baseInputGraphsPath,name)
let br = Unchecked.defaultof<JetBrains.ReSharper.Psi.CSharp.Tree.ICSharpLiteralExpression>

let getChar x =
    match x with
    | Smbl(y, _) -> y
    | _ -> failwith "Unexpected symb in alphabet of FSA!"

let printBref =
    fun x ->
        match x with
            | Yard.Examples.MSParser.DEC_NUMBER(gr) -> "NUM"
            | Yard.Examples.MSParser.IDENT(gr) -> "IDENT"
            | Yard.Examples.MSParser.L_from(gr) -> "FROM"
            | Yard.Examples.MSParser.L_select(gr) -> "SELECT"
            | Yard.Examples.MSParser.L_insert(gr) -> "INSERT"
            | Yard.Examples.MSParser.L_values(gr) -> "VALUES"
            | Yard.Examples.MSParser.L_left_bracket_(gr) -> "LEFT_BRACKET"
            | Yard.Examples.MSParser.L_right_bracket_(gr) -> "RIGHT_BRACKET"
            | Yard.Examples.MSParser.L_plus_(gr) -> "PLUS"
            | Yard.Examples.MSParser.L_more_(gr) -> "MORE"
            | Yard.Examples.MSParser.L_where(gr) -> "WHERE"
            | Yard.Examples.MSParser.L_comma_(gr) -> "COMMA"
            | Yard.Examples.MSParser.L_minus_(gr) -> "MINUS"
            | x -> string x  |> (fun s -> s.Split '+' |> Array.rev |> fun a -> a.[0]) 

let loadDotToQGReSharper baseInputGraphsPath gFile =
    let qGraph = loadGraphFromDOT(path baseInputGraphsPath gFile)
    let graphAppr = new Appr<_>()
    graphAppr.InitState <- ResizeArray.singleton 0

    for e in qGraph.Edges do
        let edg = e :?> DotEdge<string>
        new TaggedEdge<_,_>(int edg.Source.Id, int edg.Destination.Id, (edg.Label, br)) |> graphAppr.AddVerticesAndEdge |> ignore

    graphAppr.FinalState <- ResizeArray.singleton (Seq.max graphAppr.Vertices)
    graphAppr
      
let TSQLTokenizationTest path eCount vCount =
    let graphAppr = loadDotToQGReSharper baseInputGraphsPath path
    let graphFsa = graphAppr.ApprToFSA()
    let graphFst = FST<_,_>.FSAtoFST(graphFsa, transform, smblEOF)
    let res = YC.TSQLLexer.tokenize (Yard.Examples.MSParser.RNGLR_EOF(new FSA<_>())) graphFst
    match res with
    | YC.FST.GraphBasedFst.Test.Success res -> 
        checkGraph res eCount vCount  
    | YC.FST.GraphBasedFst.Test.Error e -> Assert.Fail(sprintf "Tokenization problem in test %s: %A" path e)

let test buildAstAbstract qGraph nodesCount edgesCount epsilonsCount termsCount ambiguityCount = 
    let r = (new Parser<_>()).Parse  buildAstAbstract qGraph
    //printfn "%A" r
    match r with
    | Yard.Generators.ARNGLR.Parser.Error (num, tok, message) ->
        printfn "Error in position %d on Token %A: %s" num tok message
        Assert.Fail "!!!!!!"
    | Yard.Generators.ARNGLR.Parser.Success(tree) ->
        //tree.PrintAst()
        let n, e, eps, t, amb = tree.CountCounters()
        //printfn "%A %A %A %A %A" n e eps t amb
        Assert.AreEqual(nodesCount, n, "Nodes count mismatch")
        Assert.AreEqual(edgesCount, e, "Edges count mismatch")
        Assert.AreEqual(epsilonsCount, eps, "Epsilons count mismatch")
        Assert.AreEqual(termsCount, t, "Terms count mismatch")
        Assert.AreEqual(ambiguityCount, amb, "Ambiguities count mismatch")
        Assert.Pass()

[<TestFixture>]
type ``Lexer and Parser TSQL Tests`` () =   
    [<Test>]  
    member this.``TSQL. Lexer test 1.`` () =
        TSQLTokenizationTest "test_tsql_1.dot" 9 10

    [<Test>]  
    member this.``TSQL. Lexer test 2.`` () =
        TSQLTokenizationTest "test_tsql_2.dot" 18 19 

    [<Test>]  
    member this.``TSQL. Lexer and Parser test.`` () =
        let graphAppr = loadDotToQGReSharper baseInputGraphsPath "test_tsql_1.dot"
        let graphFsa = graphAppr.ApprToFSA()
        let graphFst = FST<_,_>.FSAtoFST(graphFsa, transform, smblEOF)
        let res = YC.TSQLLexer.tokenize (Yard.Examples.MSParser.RNGLR_EOF(new FSA<_>())) graphFst
        match res with
        | YC.FST.GraphBasedFst.Test.Success res -> 
            //ToDot res @"../../../src/TSQL.Test/DotTSQL/teeeest.dot" printBref
            checkGraph res 9 10
            test  Yard.Examples.MSParser.buildAstAbstract res 156 159 5 8 4
        | YC.FST.GraphBasedFst.Test.Error e -> Assert.Fail(sprintf "Tokenization problem in test %s: %A" "test_tsql_1.dot" e)

    [<Test>]
    member this.``TSQL. Parser test 1.`` () =
        let qGraph = new ParserInputGraph<_>(0, 9)
        qGraph.AddVerticesAndEdgeRange
            [
             edg 0 1 (Yard.Examples.MSParser.L_select(new FSA<_>()))
             edg 1 2 (Yard.Examples.MSParser.IDENT(new FSA<_>()))
             edg 2 3 (Yard.Examples.MSParser.L_from(new FSA<_>()))
             edg 3 4 (Yard.Examples.MSParser.IDENT(new FSA<_>()))
             edg 4 5 (Yard.Examples.MSParser.L_where(new FSA<_>()))
             edg 5 6 (Yard.Examples.MSParser.IDENT(new FSA<_>()))
             edg 6 7 (Yard.Examples.MSParser.L_more_(new FSA<_>()))
             edg 7 8 (Yard.Examples.MSParser.IDENT(new FSA<_>()))
             edg 8 9 (Yard.Examples.MSParser.RNGLR_EOF(new FSA<_>()))
             ] |> ignore
        
        test Yard.Examples.MSParser.buildAstAbstract qGraph 156 159 5 8 4

    [<Test>]
    member this.``TSQL. Parser test 2.`` () =
        let qGraph = new ParserInputGraph<_>(0, 18)
        qGraph.AddVerticesAndEdgeRange
            [
             edg 0 1 (Yard.Examples.MSParser.L_insert(new FSA<_>()))
             edg 1 2 (Yard.Examples.MSParser.IDENT(new FSA<_>()))
             edg 1 2 (Yard.Examples.MSParser.IDENT(new FSA<_>()))
             edg 2 3 (Yard.Examples.MSParser.L_left_bracket_(new FSA<_>()))
             edg 3 4 (Yard.Examples.MSParser.IDENT(new FSA<_>()))
             edg 4 5 (Yard.Examples.MSParser.L_comma_(new FSA<_>()))
             edg 5 6 (Yard.Examples.MSParser.IDENT(new FSA<_>()))
             edg 6 7 (Yard.Examples.MSParser.L_right_bracket_(new FSA<_>()))
             edg 7 8 (Yard.Examples.MSParser.L_values(new FSA<_>()))
             edg 8 9 (Yard.Examples.MSParser.L_left_bracket_(new FSA<_>()))
             edg 9 10 (Yard.Examples.MSParser.IDENT(new FSA<_>()))
             edg 10 11 (Yard.Examples.MSParser.L_plus_(new FSA<_>()))
             edg 11 12 (Yard.Examples.MSParser.IDENT(new FSA<_>()))
             edg 12 13 (Yard.Examples.MSParser.L_comma_(new FSA<_>()))
             edg 13 14 (Yard.Examples.MSParser.IDENT(new FSA<_>()))
             edg 14 15 (Yard.Examples.MSParser.L_minus_(new FSA<_>()))
             edg 15 16 (Yard.Examples.MSParser.IDENT(new FSA<_>()))
             edg 16 17 (Yard.Examples.MSParser.L_right_bracket_(new FSA<_>()))          
             edg 17 18 (Yard.Examples.MSParser.RNGLR_EOF(new FSA<_>()))
             ] |> ignore
        
        test Yard.Examples.MSParser.buildAstAbstract qGraph 207 210 4 18 5

    [<Test>]
    member this.``TSQL. Parser test 3.`` () =
        let qGraph = new ParserInputGraph<_>(0, 15)
        qGraph.AddVerticesAndEdgeRange
            [
             edg 0 1 (Yard.Examples.MSParser.L_insert(new FSA<_>()))
             edg 1 2 (Yard.Examples.MSParser.IDENT(new FSA<_>()))             
             edg 2 3 (Yard.Examples.MSParser.L_left_bracket_(new FSA<_>()))
             edg 3 4 (Yard.Examples.MSParser.IDENT(new FSA<_>()))
             edg 4 5 (Yard.Examples.MSParser.L_comma_(new FSA<_>()))
             edg 5 6 (Yard.Examples.MSParser.IDENT(new FSA<_>()))
             edg 6 7 (Yard.Examples.MSParser.L_right_bracket_(new FSA<_>()))
             edg 7 8 (Yard.Examples.MSParser.L_values(new FSA<_>()))
             edg 8 9 (Yard.Examples.MSParser.L_left_bracket_(new FSA<_>()))
             edg 9 10 (Yard.Examples.MSParser.L_select(new FSA<_>()))
             edg 10 11 (Yard.Examples.MSParser.IDENT(new FSA<_>()))
             edg 11 12 (Yard.Examples.MSParser.L_from(new FSA<_>()))
             edg 12 13 (Yard.Examples.MSParser.IDENT(new FSA<_>()))
             edg 9 13 (Yard.Examples.MSParser.IDENT(new FSA<_>()))
             edg 13 14 (Yard.Examples.MSParser.L_right_bracket_(new FSA<_>()))
             edg 14 15 (Yard.Examples.MSParser.RNGLR_EOF(new FSA<_>()))
             ] |> ignore
        
        test Yard.Examples.MSParser.buildAstAbstract qGraph 201 203 8 15 3

//[<EntryPoint>]
//let f x =
//      let t = new ``Lexer and Parser TSQL Tests`` () 
//      t.``TSQL. Simple.``()
//      1