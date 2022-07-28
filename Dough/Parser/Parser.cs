using Sprache;
using Dough.Structure;

namespace Dough.Parser;

internal class Parser
{
    public static Unit ParseUnit(string input)
    {
        Parser<Unit> omg =
            from functions in ParseFunction().Many()
            select new Unit(functions);
        
        return omg.Parse(input);
    }
    
    private static Parser<Function> ParseFunction()
    {
        Parser<IEnumerable<Expression>> parseExpressions =
            from open in Parse.Char('{').Token()
            from expressions in ParseExpression().Many()
            from close in Parse.Char('}').Token()
            select expressions;

        return
            from keyword in Parse.String("def").Token()
            from definition in ParseDefinition()
            from expressions in parseExpressions
            select new Function(definition, expressions);
    }

    private static Parser<Expression> ParseExpression()
    {
        Parser<Expression> parsePrint =
            from keyword in Parse.String("print").Token()
            from value in ParseExpression()
            select new PrintExpression(value);

        Parser<Expression> parseLet =
            from keyword in Parse.String("let").Token()
            from definition in ParseDefinition()
            from assignment in Parse.Char('=').Token()
            from value in ParseExpression()
            select new LetExpression(definition, value);

        Parser<Expression> parseNumber =
            from value in Parse.Digit.AtLeastOnce()
            select new NumberExpression(string.Concat(value));

        Parser<Expression> parseIdentifier =
            from value in Parse.Identifier(Parse.Letter, Parse.LetterOrDigit)
            select new IdentifierExpression(string.Concat(value));

        return
            from expression in parsePrint.Or(
                               parseLet.Or(
                               parseNumber.Or(
                               parseIdentifier)))
            select expression;
    }

    private static Parser<Definition> ParseDefinition()
    {
        Parser<string> parseIdentifier = Parse.Identifier(Parse.Letter, Parse.LetterOrDigit).Token();

        Parser<string> parseType =
            from type in Parse.String("void").Or(Parse.String("i32")).Token()
            select string.Concat(type);

        return
            from identifier in parseIdentifier
            from seperator in Parse.Char(':').Token()
            from type in parseType
            select new Definition(identifier, type);
    }
}