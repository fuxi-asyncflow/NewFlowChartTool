lexer grammar NodeLexer;

// keywords
TRUE : 'true';
FALSE : 'false';
NIL : 'nil';
SELF : 'self';



// operators
ASSIGN : '=';
GT : '>';
LT : '<';
BANG : '!';
TILDE : '~';
QUESTION : '?';
COLON : ':';
EQUAL : '==';
LE : '<=';
GE : '>=';
NOTEQUAL : '!=' | 'not';
AND : '&&' | 'and';
OR : '||' | 'or';

ADD : '+';
SUB : '-';
MUL : '*';
DIV : '/';

// tokens

NAME
    : [a-zA-Z_][a-zA-Z_0-9]*
    ;

VARIABLE
    : '$' NAME
    ;

STRING
    : '\'' ( STRING_ESCAPE_SEQ | ~[\\\r\n\f'] )* '\''
    | '"' ( STRING_ESCAPE_SEQ | ~[\\\r\n\f"] )* '"'
    ;

fragment STRING_ESCAPE_SEQ
    : '\\' .
    ;

NUMBER
    : DEC_INTEGER
    | HEX_INTEGER
    | FLOAT_NUMBER
    ;

DEC_INTEGER
    : DIGIT+
    ;

HEX_INTEGER
    : '0' [xX] HEX_DIGIT+
    ;

FLOAT_NUMBER
    : POINT_FLOAT
    | EXPONENT_FLOAT
    ;

fragment DIGIT
    : [0-9]
    ;

fragment HEX_DIGIT
    : [0-9a-fA-F]
    ;

fragment POINT_FLOAT
 : INT_PART? FRACTION
 | INT_PART '.'
 ;

fragment EXPONENT_FLOAT
 : ( INT_PART | POINT_FLOAT ) EXPONENT
 ;

fragment INT_PART
 : DIGIT+
 ;

fragment FRACTION
 : '.' DIGIT+
 ;

fragment EXPONENT
 : [eE] [+-]? DIGIT+
 ;