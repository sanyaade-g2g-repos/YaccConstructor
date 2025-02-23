﻿module RNGLRAstToAstTest

open AbstractAnalysis.Common
open QuickGraph
//open Yard.Generators.RNGLR
open Yard.Generators.ARNGLR
open Yard.Generators.Common.AST
open Yard.Generators.RNGLR.AbstractParser
open Yard.Generators.RNGLR.OtherSPPF
open NUnit.Framework

let createEdge from _to label = new ParserEdge<_>(from, _to, label)

let inline printErr (num, token : 'a, msg) =
    printfn "Error in position %d on Token %A: %s" num token msg
    Assert.Fail(sprintf "Error in position %d on Token %A: %s" num token msg)

let inline translate (f : TranslateArguments<_, _> -> 'b -> 'c) (ast : 'b) =
    let args = {
        tokenToRange = fun _ -> 0,0
        zeroPosition = 0
        clearAST = false
        filterEpsilons = true
    }
    f args ast

let tokenToPos (tokenData : _ -> obj) token = 
    let t = tokenData token
    match t with
    | :? int as i -> [i] |> Seq.ofList
    | _ -> failwithf "Unexpected token data: %s" <| t.GetType().ToString()

[<TestFixture>]
type ``RNGLR ast to otherSPPF translation test`` () =

    [<Test>]
    member test.``Elementary test``() =
        let qGraph = new ParserInputGraph<_>(0, 5)
        let vertexRange = List.init 6 (fun i -> i)
        qGraph.AddVertexRange vertexRange |> ignore
        qGraph.AddVerticesAndEdgeRange
            [
                createEdge 0 1 (RNGLR.ParseElementary.A 0)
                createEdge 1 2 (RNGLR.ParseElementary.B 1)
                createEdge 2 3 (RNGLR.ParseElementary.C 2)
                createEdge 3 4 (RNGLR.ParseElementary.D 3)
                createEdge 4 5 (RNGLR.ParseElementary.RNGLR_EOF 3)
             ] |> ignore

        let parseResult = (new Parser<_>()).Parse  RNGLR.ParseElementary.buildAstAbstract qGraph
        
        match parseResult with 
        | Parser.Error (num, tok, err) -> printErr (num, tok, err)
        | Parser.Success (mAst) ->
//            RNGLR.ParseElementary.defaultAstToDot mAst "Elementary before.dot"
            let other = new OtherTree<_>(mAst)
//            RNGLR.ParseElementary.otherAstToDot other "Elementary after.dot"
            other.PrintAst()
            Assert.Pass "Elementary test: PASSED"


    [<Test>]
    member test.``Epsilon test``() =
        let qGraph = new ParserInputGraph<_>(0, 3)
        let vertexRange = List.init 4 (fun i -> i)
        qGraph.AddVertexRange vertexRange |> ignore
        qGraph.AddVerticesAndEdgeRange
            [
                createEdge 0 1 (RNGLR.ParseElementary.A 0)
                createEdge 1 2 (RNGLR.ParseElementary.C 1)
                createEdge 2 3 (RNGLR.ParseElementary.RNGLR_EOF 2)
            ] |> ignore

        let parseResult = (new Parser<_>()).Parse  RNGLR.ParseElementary.buildAstAbstract qGraph
        
        match parseResult with 
        | Parser.Error (num, tok, err) -> printErr (num, tok, err)
        | Parser.Success (mAst) ->
//            RNGLR.ParseElementary.defaultAstToDot mAst "Epsilon before.dot"
            let other = new OtherTree<_>(mAst)
//            RNGLR.ParseElementary.otherAstToDot other "Epsilon after.dot"
            other.PrintAst()
            Assert.Pass "Epsilon test: PASSED"

    [<Test>]
    member test.``Ambiguous test``() =
        let qGraph = new ParserInputGraph<_>(0, 4)

        let vertexRange = List.init 5 (fun i -> i)
        qGraph.AddVertexRange vertexRange |> ignore
        qGraph.AddVerticesAndEdgeRange
            [
                createEdge 0 1 (RNGLR.ParseAmbiguous.A 0)
                createEdge 1 2 (RNGLR.ParseAmbiguous.A 1)
                createEdge 2 3 (RNGLR.ParseAmbiguous.A 2)
                createEdge 3 4 (RNGLR.ParseAmbiguous.RNGLR_EOF 3)
            ] |> ignore

        let parseResult = (new Parser<_>()).Parse  RNGLR.ParseAmbiguous.buildAstAbstract qGraph
        
        match parseResult with 
        | Parser.Error (num, tok, err) -> printErr (num, tok, err)
        | Parser.Success (mAst) ->
//            RNGLR.ParseAmbiguous.defaultAstToDot mAst "Ambiguous before.dot"
            let other = new OtherTree<_>(mAst)
//            RNGLR.ParseAmbiguous.otherAstToDot other "Ambiguous after.dot"
            other.PrintAst()
            Assert.Pass "Ambiguous test: PASSED"

    [<Test>]
    member test.``Parents test``() =
        let qGraph = new ParserInputGraph<_>(0, 2)

        let vertexRange = List.init 3 (fun i -> i)
        qGraph.AddVertexRange vertexRange |> ignore
        qGraph.AddVerticesAndEdgeRange
            [
                createEdge 0 1 (RNGLR.ParseAmbiguous.B 0)
                createEdge 1 2 (RNGLR.ParseAmbiguous.RNGLR_EOF 1)
            ] |> ignore

        let parseResult = (new Parser<_>()).Parse  RNGLR.ParseAmbiguous.buildAstAbstract qGraph
        
        match parseResult with 
        | Parser.Error (num, tok, err) -> printErr (num, tok, err)
        | Parser.Success (mAst) ->
            let other = new OtherTree<_>(mAst)
//            RNGLR.ParseAmbiguous.otherAstToDot other "Parents after.dot"
            other.PrintAst()
            Assert.Pass "Parents test: PASSED"

    [<Test>]
    member test.``Cycles test``() =
        let qGraph = new ParserInputGraph<_>(0, 2)
        
        let vertexRange = List.init 3 (fun i -> i)
        qGraph.AddVertexRange vertexRange |> ignore
        qGraph.AddVerticesAndEdgeRange
            [
                createEdge 0 1 (RNGLR.ParseCycles.A 0)
                createEdge 1 2 (RNGLR.ParseCycles.RNGLR_EOF 1)
            ] |> ignore

        let parseResult = (new Parser<_>()).Parse  RNGLR.ParseCycles.buildAstAbstract qGraph
        
        match parseResult with 
        | Parser.Error (num, tok, err) -> printErr (num, tok, err)
        | Parser.Success (mAst) ->
//            RNGLR.ParseCycles.defaultAstToDot mAst "Cycles before.dot"

            let other = new OtherTree<_>(mAst)
            
//            RNGLR.ParseCycles.otherAstToDot other "Cycles after.dot"
            other.PrintAst()
            Assert.Pass "Cycles test: PASSED"

[<TestFixture>]
type ``Classic case: matching brackets``() =
    let tokToNumber = RNGLR.ParseSummator.tokenToNumber
    let leftBraceNumber  = tokToNumber <| RNGLR.ParseSummator.Token.LBRACE -1
    let rightBraceNumber = tokToNumber <| RNGLR.ParseSummator.Token.RBRACE -1
    let tokToPos = tokenToPos RNGLR.ParseSummator.tokenData
    
    let errorMessage = "Expected bracket wasn't found"

    [<Test>]
    member test.``Classic case. Simple test``() =
        let qGraph = new ParserInputGraph<_>(0, 6)
        
        let vertexRange = List.init 7 (fun i -> i)
        qGraph.AddVertexRange vertexRange |> ignore
        qGraph.AddVerticesAndEdgeRange
            [
                createEdge 0 1 (RNGLR.ParseSummator.LBRACE 0)
                createEdge 1 2 (RNGLR.ParseSummator.NUMBER 1)
                createEdge 2 3 (RNGLR.ParseSummator.PLUS 2)
                createEdge 3 4 (RNGLR.ParseSummator.NUMBER 3)
                createEdge 4 5 (RNGLR.ParseSummator.RBRACE 4)
                createEdge 5 6 (RNGLR.ParseSummator.RNGLR_EOF 5)
            ] |> ignore

        let parseResult = (new Parser<_>()).Parse  RNGLR.ParseSummator.buildAstAbstract qGraph
        
        match parseResult with 
        | Parser.Error (num, tok, err) -> printErr (num, tok, err)
        | Parser.Success (mAst) ->
            let other = new OtherTree<_>(mAst)
            
            let pairTokens = other.FindAllPair leftBraceNumber rightBraceNumber 0 true tokToNumber tokToPos

            let expectedPairs = 1
            Assert.AreEqual (expectedPairs, pairTokens.Count)

            
            let actual = 
                match pairTokens.[0] with
                | RNGLR.ParseSummator.RBRACE pos -> pos
                | _ -> 
                    let token = RNGLR.ParseSummator.numToString <| tokToNumber pairTokens.[0]
                    Assert.Fail <| sprintf "%s was found" token
                    -1
            
            let expectedPos = 4
            Assert.AreEqual (expectedPos, actual, errorMessage)
            Assert.Pass "Classic case. Simple test: PASSED"

    [<Test>]
    member test.``Classic case. Many brackets 1``() =
        let qGraph = new ParserInputGraph<_>(0, 6)
        
        let vertexRange = List.init 7 (fun i -> i)
        qGraph.AddVertexRange vertexRange |> ignore
        qGraph.AddVerticesAndEdgeRange
            [
                createEdge 0 1 (RNGLR.ParseSummator.LBRACE 0)
                createEdge 1 2 (RNGLR.ParseSummator.LBRACE 1)
                createEdge 2 3 (RNGLR.ParseSummator.NUMBER 2)
                createEdge 3 4 (RNGLR.ParseSummator.RBRACE 3)
                createEdge 4 5 (RNGLR.ParseSummator.RBRACE 4)
                createEdge 5 6 (RNGLR.ParseSummator.RNGLR_EOF 5)
            ] |> ignore

        let parseResult = (new Parser<_>()).Parse  RNGLR.ParseSummator.buildAstAbstract qGraph
        
        match parseResult with 
        | Parser.Error (num, tok, err) -> printErr (num, tok, err)
        | Parser.Success (mAst) ->
            let other = new OtherTree<_>(mAst)

            let expectedPairs = 1
            let pairTokens = other.FindAllPair leftBraceNumber rightBraceNumber 0 true tokToNumber tokToPos

            Assert.AreEqual (expectedPairs, pairTokens.Count)
            
            let actual = 
                match pairTokens.[0] with
                | RNGLR.ParseSummator.RBRACE pos -> pos
                | _ -> 
                    let token = RNGLR.ParseSummator.numToString <| tokToNumber pairTokens.[0]
                    Assert.Fail <| sprintf "%s was founded" token
                    -1
            
            let expectedPos = 4
            Assert.AreEqual (expectedPos, actual, errorMessage)
            Assert.Pass "Classic case. Many brackets 1: PASSED"

    [<Test>]
    member test.``Classic case. Many brackets 2``() =
        let qGraph = new ParserInputGraph<_>(0, 6)
        
        let vertexRange = List.init 7 (fun i -> i)
        qGraph.AddVertexRange vertexRange |> ignore
        qGraph.AddVerticesAndEdgeRange
            [
                createEdge 0 1 (RNGLR.ParseSummator.LBRACE 0)
                createEdge 1 2 (RNGLR.ParseSummator.LBRACE 1)
                createEdge 2 3 (RNGLR.ParseSummator.NUMBER 2)
                createEdge 3 4 (RNGLR.ParseSummator.RBRACE 3)
                createEdge 4 5 (RNGLR.ParseSummator.RBRACE 4)
                createEdge 5 6 (RNGLR.ParseSummator.RNGLR_EOF 5)
            ] |> ignore

        let parseResult = (new Parser<_>()).Parse RNGLR.ParseSummator.buildAstAbstract qGraph
        
        match parseResult with 
        | Parser.Error (num, tok, err) -> printErr (num, tok, err)
        | Parser.Success (mAst) ->
            
            let other = new OtherTree<_>(mAst)

            let expectedPairs = 1
            let pairTokens = other.FindAllPair leftBraceNumber rightBraceNumber 1 true tokToNumber tokToPos

            Assert.AreEqual (expectedPairs, pairTokens.Count)
            
            let actual = 
                match pairTokens.[0] with
                | RNGLR.ParseSummator.RBRACE pos -> pos
                | _ -> 
                    let token = RNGLR.ParseSummator.numToString <| tokToNumber pairTokens.[0]
                    Assert.Fail <| sprintf "%s was found" token
                    -1
            
            let expectedPos = 3
            Assert.AreEqual (expectedPos, actual, errorMessage)
            Assert.Pass "Classic case. Many brackets 2: PASSED"

    [<Test>]
    member test.``Classic case. Right to left 1``() =
        let qGraph = new ParserInputGraph<_>(0, 6)
        
        let vertexRange = List.init 7 (fun i -> i)
        qGraph.AddVertexRange vertexRange |> ignore
        qGraph.AddVerticesAndEdgeRange
            [
                createEdge 0 1 (RNGLR.ParseSummator.LBRACE 0)
                createEdge 1 2 (RNGLR.ParseSummator.LBRACE 1)
                createEdge 2 3 (RNGLR.ParseSummator.NUMBER 2)
                createEdge 3 4 (RNGLR.ParseSummator.RBRACE 3)
                createEdge 4 5 (RNGLR.ParseSummator.RBRACE 4)
                createEdge 5 6 (RNGLR.ParseSummator.RNGLR_EOF 5)
            ] |> ignore

        let result = (new Parser<_>()).Parse  RNGLR.ParseSummator.buildAstAbstract qGraph
        match result with
        | Parser.Error (num, tok, message) -> printErr (num, tok, message)
        | Parser.Success(mAst) ->
            let other = new OtherTree<_>(mAst)
            
            let pairTokens = other.FindAllPair leftBraceNumber rightBraceNumber 3 false tokToNumber tokToPos
            
            let expectedPairs = 1
            Assert.AreEqual (expectedPairs, pairTokens.Count)

            
            let actual = 
                match pairTokens.[0] with
                | RNGLR.ParseSummator.LBRACE pos -> pos
                | _ -> 
                    let token = RNGLR.ParseSummator.numToString <| tokToNumber pairTokens.[0]
                    Assert.Fail <| sprintf "%s was found" token
                    -1

            let expectedPos = 1
            Assert.AreEqual (expectedPos, actual, errorMessage)
            Assert.Pass <| "Classic case. Right to left: PASSED"

    [<Test>]
    member test.``Classic case. Right to left 2``() =
        let qGraph = new ParserInputGraph<_>(0, 6)
        
        let vertexRange = List.init 7 (fun i -> i)
        qGraph.AddVertexRange vertexRange |> ignore
        qGraph.AddVerticesAndEdgeRange
            [
                createEdge 0 1 (RNGLR.ParseSummator.LBRACE 0)
                createEdge 1 2 (RNGLR.ParseSummator.LBRACE 1)
                createEdge 2 3 (RNGLR.ParseSummator.NUMBER 2)
                createEdge 3 4 (RNGLR.ParseSummator.RBRACE 3)
                createEdge 4 5 (RNGLR.ParseSummator.RBRACE 4)
                createEdge 5 6 (RNGLR.ParseSummator.RNGLR_EOF 5)
            ] |> ignore

        let result = (new Parser<_>()).Parse  RNGLR.ParseSummator.buildAstAbstract qGraph
        match result with
        | Parser.Error (num, tok, message) -> printErr (num, tok, message)
        | Parser.Success(mAst) ->
            let other = new OtherTree<_>(mAst)

            RNGLR.ParseSummator.otherAstToDot other "Classic case right to left 2.dot"

            let pairTokens = other.FindAllPair leftBraceNumber rightBraceNumber 4 false tokToNumber tokToPos
            
            let expectedCount = 1
            Assert.AreEqual (expectedCount, pairTokens.Count)

            let actual = 
                match pairTokens.[0] with
                | RNGLR.ParseSummator.LBRACE pos -> pos
                | _ -> 
                    let token = RNGLR.ParseSummator.numToString <| tokToNumber pairTokens.[0]
                    Assert.Fail <| sprintf "%s was found" token
                    -1
            
            let expected = 0
            Assert.AreEqual (expected, actual, errorMessage)
            Assert.Pass "Classic case. Right to left 2: PASSED"

[<TestFixture>]
type ``Abstract case: matching brackets``() =
    let tokToNumber = RNGLR.ParseSummator.tokenToNumber
    let leftBraceNumber  = tokToNumber <| RNGLR.ParseSummator.Token.LBRACE -1
    let rightBraceNumber = tokToNumber <| RNGLR.ParseSummator.Token.RBRACE -1
    let tokToPos = tokenToPos RNGLR.ParseSummator.tokenData
    let parse graph = (new Parser<_>()).Parse  RNGLR.ParseSummator.buildAstAbstract graph

    let infoAboutError = "Some expected brackets weren't found"

    let NotBracketIsFound token = 
        let tokenName = RNGLR.ParseSummator.numToString <| tokToNumber token
        Assert.Fail <| sprintf "%s is found" tokenName
        -1

    [<Test>]
    member test.``Abstract case. Left to right. Two parentheses 1``() =
        let qGraph = new ParserInputGraph<_>(0, 4)
        printfn "                       | --> ')'"
        printfn "[caret]'(' --> '1' --> |        "
        printfn "                       | --> ')'"
        
        let vertexRange = List.init 5 (fun i -> i)
        qGraph.AddVertexRange vertexRange |> ignore
        qGraph.AddVerticesAndEdgeRange
            [
                createEdge 0 1 (RNGLR.ParseSummator.LBRACE 0)
                createEdge 1 2 (RNGLR.ParseSummator.NUMBER 1)
                createEdge 2 3 (RNGLR.ParseSummator.RBRACE 2)
                createEdge 2 3 (RNGLR.ParseSummator.RBRACE 3)
                createEdge 3 4 (RNGLR.ParseSummator.RNGLR_EOF 4)
            ] |> ignore

        let result = parse qGraph
        match result with
        | Parser.Error (num, tok, message) -> printErr (num, tok, message)
        | Parser.Success(mAst) ->
            
            let expected = [|2; 3|]
            
            let other = new OtherTree<_>(mAst)

            RNGLR.ParseSummator.defaultAstToDot mAst "ast.dot"
            RNGLR.ParseSummator.otherAstToDot other "other.dot"
            let pairTokens = other.FindAllPair leftBraceNumber rightBraceNumber 0 true tokToNumber tokToPos

            Assert.AreEqual (expected.Length, pairTokens.Count)

            let actual = 
                pairTokens 
                |> Seq.map 
                    (fun token -> 
                        match token with
                        | RNGLR.ParseSummator.RBRACE pos -> pos
                        | _ -> NotBracketIsFound token
                    )
                |> Seq.sort
                |> Array.ofSeq        

            Assert.AreEqual (expected, actual, infoAboutError)
            Assert.Pass "Abstract case. Left to right. Two parentheses 1 PASSED"

    [<Test>]
    member test.``Abstract case. Left to right. Two parentheses 2``() =
        let qGraph = new ParserInputGraph<_>(0, 5)
        printfn "     |--> 1 --> ')'"
        printfn "( -->|             "
        printfn "     |--> 2 --> ')'"
        
        let vertexRange = List.init 6 (fun i -> i)
        qGraph.AddVertexRange vertexRange |> ignore
        qGraph.AddVerticesAndEdgeRange
            [
                createEdge 0 1 (RNGLR.ParseSummator.LBRACE 0)
                createEdge 1 2 (RNGLR.ParseSummator.NUMBER 1)
                createEdge 1 3 (RNGLR.ParseSummator.NUMBER 2)
                createEdge 2 4 (RNGLR.ParseSummator.RBRACE 3)
                createEdge 3 4 (RNGLR.ParseSummator.RBRACE 4)
                createEdge 4 5 (RNGLR.ParseSummator.RNGLR_EOF 5)
            ] |> ignore

        let result = parse qGraph
        match result with
        | Parser.Error (num, tok, message) -> printErr (num, tok, message)
        | Parser.Success(mAst) ->
            let other = new OtherTree<_>(mAst)
            
            let pairTokens = other.FindAllPair leftBraceNumber rightBraceNumber 0 true tokToNumber tokToPos

            let expected = [|3; 4|]

            Assert.AreEqual (expected.Length, pairTokens.Count)
            
            let actual = 
                pairTokens 
                |> Seq.map 
                    (fun token -> 
                        match token with
                        | RNGLR.ParseSummator.RBRACE pos -> pos
                        | _ -> NotBracketIsFound token
                     )
                |> Seq.sort
                |> Array.ofSeq

            Assert.AreEqual (expected, actual, infoAboutError)
            Assert.Pass "Abstract case. Left to right. Two parentheses 2 PASSED"

    [<Test>]
    member test.``Abstract case. Right to left. Two parentheses 1``() =
        printfn " '(' --> "
        printfn "         '2' --> ')'[caret]"
        printfn " '(' --> "
        
        let qGraph = new ParserInputGraph<_>(0, 4)
        let vertexRange = List.init 5 (fun i -> i)
        qGraph.AddVertexRange vertexRange |> ignore
        qGraph.AddVerticesAndEdgeRange
            [
                createEdge 0 1 (RNGLR.ParseSummator.LBRACE 0)
                createEdge 0 1 (RNGLR.ParseSummator.LBRACE 1)
                createEdge 1 2 (RNGLR.ParseSummator.NUMBER 2)
                createEdge 2 3 (RNGLR.ParseSummator.RBRACE 3)
                createEdge 3 4 (RNGLR.ParseSummator.RNGLR_EOF 4)
            ] |> ignore

        let result = parse qGraph
        
        match result with
        | Parser.Error (num, tok, message) -> printErr (num, tok, message)
        | Parser.Success(mAst) ->
            
            let other = new OtherTree<_>(mAst)
            
            let pairTokens = other.FindAllPair leftBraceNumber rightBraceNumber 3 false tokToNumber tokToPos
            
            let expected = [|0; 1;|]
            Assert.AreEqual (expected.Length, pairTokens.Count)

            let actual = 
                pairTokens 
                |> Seq.map 
                    (fun token -> 
                        match token with
                        | RNGLR.ParseSummator.LBRACE pos -> pos
                        | _ -> NotBracketIsFound token
                     )
                |> Seq.sort
                |> Array.ofSeq

            Assert.AreEqual (expected, actual, infoAboutError)
            Assert.Pass "Abstract case. Right to left. Two parentheses 1 PASSED"

    [<Test>]
    member test.``Abstract case. Right to left. Two parentheses 2``() =
        let qGraph = new ParserInputGraph<_>(0, 5)
        printfn " '(' --> '2' -->"
        printfn "               ')'[caret]"
        printfn " '(' --> '3' -->"
        
        let vertexRange = List.init 6 (fun i -> i)
        qGraph.AddVertexRange vertexRange |> ignore
        qGraph.AddVerticesAndEdgeRange
            [
                createEdge 0 1 (RNGLR.ParseSummator.LBRACE 0)
                createEdge 0 2 (RNGLR.ParseSummator.LBRACE 1)
                createEdge 1 3 (RNGLR.ParseSummator.NUMBER 2)
                createEdge 2 3 (RNGLR.ParseSummator.NUMBER 3)
                createEdge 3 4 (RNGLR.ParseSummator.RBRACE 4)
                createEdge 4 5 (RNGLR.ParseSummator.RNGLR_EOF 5)
            ] |> ignore

        let result = parse qGraph
        
        match result with
        | Parser.Error (num, tok, message) -> printErr (num, tok, message)
        | Parser.Success(mAst) ->
            
            let other = new OtherTree<_>(mAst)
            
            let pairTokens = other.FindAllPair leftBraceNumber rightBraceNumber 4 false tokToNumber tokToPos
            
            let expected = [|0; 1;|]
            Assert.AreEqual (expected.Length, pairTokens.Count)

            let actual = 
                pairTokens 
                |> Seq.map 
                    (fun token -> 
                        match token with
                        | RNGLR.ParseSummator.LBRACE pos -> pos
                        | _ -> NotBracketIsFound token
                     )
                |> Seq.sort
                |> Array.ofSeq

            Assert.AreEqual (expected, actual, infoAboutError)
            Assert.Pass "Abstract case. Right to left. Two parentheses 2 PASSED"

    [<Test>]
    member test.``Abstract case. Right to left. One parenthesis 1``() =
        printfn "        '(' -> "
        printfn " '(' ->        '3' -> ')' -> ')'[caret]"
        printfn "        '(' -> "
        
        let qGraph = new ParserInputGraph<_>(0, 6)
        let vertexRange = List.init 7 (fun i -> i) 
        qGraph.AddVertexRange vertexRange |> ignore
        qGraph.AddVerticesAndEdgeRange
            [
                createEdge 0 1 (RNGLR.ParseSummator.LBRACE 0)
                createEdge 1 2 (RNGLR.ParseSummator.LBRACE 1)
                createEdge 1 2 (RNGLR.ParseSummator.LBRACE 2)
                createEdge 2 3 (RNGLR.ParseSummator.NUMBER 3)
                createEdge 3 4 (RNGLR.ParseSummator.RBRACE 4)
                createEdge 4 5 (RNGLR.ParseSummator.RBRACE 5)
                createEdge 5 6 (RNGLR.ParseSummator.RNGLR_EOF 6)
            ] |> ignore

        let result = parse qGraph
        
        match result with
        | Parser.Error (num, tok, message) -> printErr (num, tok, message)
        | Parser.Success(mAst) ->
            
            let other = new OtherTree<_>(mAst)
            let pairTokens = other.FindAllPair leftBraceNumber rightBraceNumber 5 false tokToNumber tokToPos
            
            let expectedPairs = 1
            Assert.AreEqual (expectedPairs, pairTokens.Count)

            let actual = 
                match pairTokens.[0] with
                | RNGLR.ParseSummator.LBRACE pos -> pos
                | _ -> NotBracketIsFound pairTokens.[0]

            let expectedPos = 0
            Assert.AreEqual (expectedPos, actual, infoAboutError)
            Assert.Pass "Abstract case. Right to left. One parenthesis 1 PASSED"

//[<EntryPoint>]
let f x = 
    let elementary = new ``RNGLR ast to otherSPPF translation test``()
//    elementary.``Parents test``()
//    elementary.``Elementary test``()
//    elementary.``Epsilon test``()
//    elementary.``Ambiguous test``()
//    elementary.``Cycles test``()

    let classic = new ``Classic case: matching brackets``()
//    classic.``Classic case. Right to left 2``()
//    classic.``Classic case. Many brackets 1``()
//    classic.``Classic case. Simple test``()

//    let brackets = new ``Abstract case: matching brackets``()
//    brackets.``More complicated test``()
//    brackets.``Many brackets 1``()
//    brackets.``Many brackets 2``()
//    brackets.``AbstractAnalysis case``()
//    brackets.``Abstract case. Two parentheses 2``()
//    brackets.``Simple right to left``()
//    brackets.``AbstractAnalysis case. Right to left``()
//    brackets.``Abstract case. Right to left``()

    let abstr = new ``Abstract case: matching brackets``()
    abstr.``Abstract case. Left to right. Two parentheses 1``()
//    abstr.``Abstract case. Two parentheses``()
//    abstr.``Abstract case. Two parentheses light``()
    1