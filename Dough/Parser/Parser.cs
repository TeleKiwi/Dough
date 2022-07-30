using Sprache;
using Dough.Structure;

namespace Dough.Parser;

internal class Parser
{
    public static Unit ParseUnit(string input)
    {
        Parser<Unit> unit =
            from functions in ParseFunction().Many()
            select new Unit(functions);
        
        return unit.Parse(input);
    }
    
    private static Parser<Function> ParseFunction()
    {
        Parser<IEnumerable<Expression>> parseExpressions =
            from open in Parse.Char('{').Token()
            from expressions in ParseExpression().Many()
            from close in Parse.Char('}').Token()
            select expressions;

        return
            from keyword in ParseString("def").Token()
            from definition in ParseDefinition()
            from expressions in parseExpressions
            select new Function(definition, expressions);
    }


    private static Parser<Expression> ParseExpression()
    {
        Parser<Expression> parsePrint =
            from keyword in ParseString("print").Token()
            from value in ParseExpression()
            select new PrintExpression(value);

        Parser<Expression> parseLet =
            from keyword in ParseString("let").Token()
            from definition in ParseDefinition()
            from assignment in Parse.Char('=').Token()
            from value in ParseExpression()
            select new LetExpression(definition, value);

        Parser<Expression> parseIfElse =
            from keyword in ParseString("if").Token()
            from condition in ParseExpression()
            from thenKeyword in ParseString("then").Token()
            from onTrue in ParseExpression()
            from elseKeyword in ParseString("else").Token()
            from onFalse in ParseExpression()
            select new IfElseExpression(condition, onTrue, onFalse);

        Parser<Expression> parseReturn =
            from keyword in ParseString("return").Token()
            from value in ParseExpression().Or(Parse.Return<Expression?>(null))
            select new ReturnExpression(value);

        Parser<Expression> parseCall =
            from identifier in Parse.Identifier(Parse.Letter, Parse.LetterOrDigit).Token().Where((i) => !IsReserved(i))
            from arguments in ParseExpression().DelimitedBy(Parse.Char(',').Token()).Optional().Contained(Parse.Char('(').Token(), Parse.Char(')').Token())
            select new CallExpression(string.Concat(identifier), arguments.GetOrElse(Enumerable.Empty<Expression>()));

        Parser<Expression> parseNumber =
            from value in Parse.Digit.AtLeastOnce().Token()
            select new NumberExpression(string.Concat(value));

        Parser<Expression> parseIdentifier =
            from identifier in Parse.Identifier(Parse.Letter, Parse.LetterOrDigit).Token().Where((i) => !IsReserved(i))
            select new IdentifierExpression(string.Concat(identifier));

        Parser<Expression> parseParenthesis =
            from open in Parse.Char('(').Token()
            from value in ParseExpression()
            from close in Parse.Char(')').Token()
            select value;

        Parser<Expression> primaryParser =
            parsePrint.Or(
            parseLet.Or(
            parseIfElse.Or(
            parseReturn.Or(
            parseCall.Or(
            parseNumber.Or(
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

    private static Parser<Definition> ParseDefinition()
    {
        Parser<string> parseIdentifier = Parse.Identifier(Parse.Letter, Parse.LetterOrDigit).Token().Where((i) => !IsReserved(i));

        Parser<Type> parseType =
            from type in ParseString("void").Or(ParseString("i32")).Token()
            from parameters in (
                from open in Parse.Char('(').Token()
                from definitions in Parse.DelimitedBy(ParseDefinition(), Parse.Char(',').Token()).Or(Parse.Return(Enumerable.Empty<Definition>()))
                from close in Parse.Char(')').Token()
                select definitions
            ).Or(Parse.Return<IEnumerable<Definition>?>(null))
            select new Type(type, parameters);

        return
            from identifier in parseIdentifier
            from seperator in Parse.Char(':').Token()
            from type in parseType
            select new Definition(identifier, type);
    }

    private static Parser<string> ParseString(string value)
    {
        return Parse.Identifier(Parse.Letter, Parse.LetterOrDigit).Token().Where((v) => v == value);
    }

    private static bool IsReserved(string value)
    {
        return value is
            "print" or
            "let" or
            "if" or
            "then" or
            "else" or
            "return" or
            "void" or
            "i32";
    }
}