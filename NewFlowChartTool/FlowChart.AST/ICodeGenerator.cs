﻿using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FlowChart.AST.Nodes;

namespace FlowChart.AST
{
    public class GenerateContent
    {
        public enum ContentType
        {
            ERROR = 0,
            FUNC = 1,
            TIMER = 2,
            EVENT = 3,
            CONTROL = 4,
            COMMENT = 5,
        }

        public GenerateContent()
        {
            Type = ContentType.ERROR;
            Contents = new List<string>();
        }

        public ContentType Type;
        public List<string> Contents;
        public string? ReturnVarName;
    }

    public class ParseException : Exception
    {
        public ParseException(string msg) : base(msg)
        {
            
        }
    }
    public class ParseResult
    {
        public ParseResult()
        {
            Content = new GenerateContent();
        }
        public bool IsWait => !string.IsNullOrEmpty(EventName);
        public bool IsAction;
        public bool IsError => !string.IsNullOrEmpty(ErrorMessage);
        public bool IsWarning => !string.IsNullOrEmpty(WarningMessage);
        public string? ErrorMessage { get; set; }
        public string? WarningMessage { get; set; }
        public string? EventName { get; set; }
        public GenerateContent Content { get; set; }
        public List<TextToken>? Tokens { get; set; }
        public object? Type { get; set; }
        public bool IsAsync { get; set; }
        public string SubGraphName { get; set; }
    }

    public class TextToken
    {
        public enum TokenType
        {
            Default = 0,
            Variable = 1,
            Member = 2,
            Number = 3,
            String = 4,
            Superscript = 5,
            End,
        }
        public int Start;
        public int End;
        public TokenType Type;
    }

    public class ParserConfig
    {
        public ParserConfig()
        {
            GetTokens = false;
            OnlyGetType = false; // used for autoclomplete
        }

        public bool GetTokens;
        public bool OnlyGetType;
    }

    public class ParserError
    {
        public ParserError(string msg)
        {
            Message = msg;
        }

        public string Message;
        public int Position;
        public int Line;
        public bool IsWarning;
    }

    public interface IParser
    {
        public ASTNode? Parse(string text, ParserConfig cfg);
        public List<TextToken>? Tokens { get; }
        public ParserError? Error { get; }
    }

    public interface IASTNodeVisitor<T>
    {
        T Visit(NumberNode node);
        T Visit(StringNode node);
        T Visit(VariableNode node);
        T Visit(BoolNode node);
        T Visit(SelfNode node);
        T Visit(NullNode node);
        T Visit(NameNode node);
        T Visit(UnaryOpNode node);
        T Visit(BinOpNode node);
        T Visit(ArgNode node);
        T Visit(ArgListNode node);
        T Visit(FuncNode node);
        T Visit(MemberNode node);
        T Visit(SubscriptNode node);
        T Visit(AssignmentNode node);
        T Visit(ContainerNode node);
        T Visit(ParenthesisNode node);

    }


}
