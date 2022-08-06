using Sprache;
using Dough.Structure;

namespace Dough.Parser;

internal class Parser
{
    public static Unit ParseUnit(string input)
    {
        Parser<Unit> unit =
            from functions in ParseFunction().Or(ParseExtern()).Many()
            select new Unit(functions.ToArray());
        
        return unit.Parse(input);
    }

    private static Parser<Function> ParseExtern()
    {
        Parser<string> parseString =
            from open in Parse.Char('\"').Token()
            from value in Parse.CharExcept('\"').AtLeastOnce()
            from close in Parse.Char('\"').Token()
            select string.Concat(value);

        return
            from @extern in ParseKeyword("extern")
            from identifier in ParseIdentifier()
            from type in (
                from colon in Parse.Char(':').Token()
                from type in ParseType()
                select type
            )
            from @from in ParseKeyword("from")
            from file in parseString
            select new ExternalFunction(identifier, (type as FunctionType)!, file);
    }
    
    private static Parser<Function> ParseFunction()
    {
        Parser<IEnumerable<Expression>> parseExpressions =
            from open in Parse.Char('{').Token()
            from expressions in ParseExpression().Many()
            from close in Parse.Char('}').Token()
            select expressions;

        return
            from keyword in ParseKeyword("def").Token()
            from identifier in ParseIdentifier()
            from type in (
                from colon in Parse.Char(':').Token()
                from type in ParseType()
                select type
            )
            from expressions in parseExpressions
            select new Function(identifier, (type as FunctionType)!, expressions.ToArray());
    }


    private static Parser<Expression> ParseExpression()
    {
        /*Parser<Expression> parsePrint =
            from keyword in ParseKeyword("print").Token()
            from value in ParseExpression()
            select new PrintExpression(value);*/

        Parser<Expression> parseLet =
            from keyword in ParseKeyword("let").Token()
            from identifier in ParseIdentifier()
            from type in (
                from colon in Parse.Char(':').Token()
                from type in ParseType()
                select type
            ).Optional()
            from assignment in Parse.Char('=').Token()
            from value in ParseExpression()
            select new LetExpression(identifier, type.GetOrElse(Type.Unknown()), value);

        Parser<Expression> parseIfElse =
            from keyword in ParseKeyword("if").Token()
            from condition in ParseExpression()
            from thenKeyword in ParseKeyword("then").Token()
            from onTrue in ParseExpression()
            from elseKeyword in ParseKeyword("else").Token()
            from onFalse in ParseExpression()
            select new IfElseExpression(condition, onTrue, onFalse);

        Parser<Expression> parseReturn =
            from keyword in ParseKeyword("return").Token()
            from value in ParseExpression().Or(Parse.Return<Expression?>(null))
            select new ReturnExpression(value);

        Parser<Expression> parseCall =
            from identifier in Parse.Identifier(Parse.Letter, Parse.LetterOrDigit).Token().Where((i) => !IsKeyword(i))
            from arguments in ParseExpression().DelimitedBy(Parse.Char(',').Token()).Optional().Contained(Parse.Char('(').Token(), Parse.Char(')').Token())
            select new CallExpression(string.Concat(identifier), arguments.GetOrElse(Enumerable.Empty<Expression>()).ToArray());

        Parser<Expression> parseNumber =
            from value in Parse.Digit.AtLeastOnce().Token()
            select new NumberExpression(string.Concat(value));

        Parser<Expression> parseString =
            from open in Parse.Char('\"').Token()
            from value in Parse.CharExcept('\"').AtLeastOnce()
            from close in Parse.Char('\"').Token()
            select new StringExpression(string.Concat(value));

        Parser<Expression> parseIdentifier =
            from identifier in ParseIdentifier()
            select new IdentifierExpression(identifier);

        Parser<Expression> parseParenthesis =
            from open in Parse.Char('(').Token()
            from value in ParseExpression()
            from close in Parse.Char(')').Token()
            select value;

        Parser<Expression> primaryParser =
            /*parsePrint.Or(*/
            parseLet.Or(
            parseIfElse.Or(
            parseReturn.Or(
            parseCall.Or(
            parseNumber.Or(
            parseString.Or(
            parseIdentifier.Or(
            parseParenthesis
            )))))));

        Parser<Expression> multiplicativeParser = Parse.ChainOperator(
            Parse.Char('*').Or(Parse.Char('/').Or(Parse.Char('%'))).Token(),
            primaryParser,
            (op, l, r) => new BinaryExpression(l, op.ToString(), r)
            );

        Parser<Expression> additiveParser = Parse.ChainOperator(
            Parse.Char('+').Or(Parse.Char('-')).Token(), 
            multiplicativeParser, 
            (op, l, r) => new BinaryExpression(l, op.ToString(), r)
            );

        Parser<Expression> conditionalParser = Parse.ChainOperator(
            Parse.String("==").Token(),
            additiveParser,
            (op, l, r) => new BinaryExpression(l, string.Concat(op), r)
            );

        Parser<Expression> assignmentParser = Parse.ChainRightOperator(
            Parse.Char('=').Token(),
            conditionalParser,
            (op, l, r) => new AssignmentExpression(l, r)
            );

        return assignmentParser;
    }

    private static Parser<string> ParseIdentifier()
    {
        return Parse.Identifier(Parse.Letter, Parse.LetterOrDigit).Token().Where((i) => !IsKeyword(i));
    }

    private static Parser<Type> ParseType()
    {
        return
            from type in
                ParseKeyword("void").Return(Type.Void())
                .Or(ParseKeyword("i32").Return(Type.I32()))
                .Or(ParseKeyword("string").Return(Type.String()))
            from parameters in (
                from open in Parse.Char('(').Token()
                from parameters in (
                    from identifier in ParseIdentifier()
                    from colon in Parse.Char(':').Token()
                    from type in ParseType()
                    select (identifier, type)
                ).DelimitedBy(Parse.Char(',').Token()).Or(Parse.Return(Enumerable.Empty<(string, Type)>()))
                from close in Parse.Char(')').Token()
                select parameters
            ).Optional()
            select (Type)(parameters.IsEmpty ? (type as CoreType)! : new FunctionType((type as CoreType)!, parameters.GetOrDefault().ToArray()));
    }

    private static Parser<string> ParseKeyword(string value)
    {
        return Parse.Identifier(Parse.Letter, Parse.LetterOrDigit).Token().Where((v) => v == value);
    }

    private static bool IsKeyword(string value)
    {
        return value is
            "extern" or
            "from" or
            "def" or
            //"print" or
            "let" or
            "if" or
            "then" or
            "else" or
            "return" or
            "void" or
            "i32" or
            "string";
    }
}