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

        Parser<Expression> parseIfElse =
            from keyword in Parse.String("if").Token()
            from condition in ParseExpression()
            from thenKeyword in Parse.String("then").Token()
            from onTrue in ParseExpression()
            from elseKeyword in Parse.String("else").Token()
            from onFalse in ParseExpression()
            select new IfElseExpression(condition, onTrue, onFalse);

        Parser<Expression> parseNumber =
            from value in Parse.Digit.AtLeastOnce().Token()
            select new NumberExpression(string.Concat(value));

        Parser<Expression> parseIdentifier =
            from value in Parse.Identifier(Parse.Letter, Parse.LetterOrDigit).Token()
            select new IdentifierExpression(string.Concat(value));

        Parser<Expression> parseParenthesis =
            from open in Parse.Char('(').Token()
            from value in ParseExpression()
            from close in Parse.Char(')').Token()
            select value;

        Parser<Expression> primaryParser =
            parsePrint.Or(
            parseLet.Or(
            parseIfElse.Or(
            parseNumber.Or(
            parseIdentifier.Or(
            parseParenthesis)))));

        Parser<Expression> multiplicativeParser = Parse.ChainOperator(
            Parse.Char('*').Or(Parse.Char('/')).Token(),
            primaryParser,
            (op, l, r) => new BinaryExpression(l, op.ToString(), r)
            );

        Parser<Expression> additiveParser = Parse.ChainOperator(
            Parse.Char('+').Or(Parse.Char('-')).Token(), 
            multiplicativeParser, 
            (op, l, r) => new BinaryExpression(l, op.ToString(), r)
            );

        Parser<Expression> assignmentParser = Parse.ChainRightOperator(
            Parse.Char('=').Token(),
            additiveParser,
            (op, l, r) => new AssignmentExpression(l, r)
            );

        return assignmentParser;
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