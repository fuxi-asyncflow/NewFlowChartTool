grammar NodeParser;
import NodeLexer;

stat
    : assign_stat                   # stat_assign
    | expr                          # stat_expr
    ;

// top has high priority
expr
    : atom_expr                     # expr_atom
    | NAME                          # expr_name
    | container_expr                # expr_container
    | '(' expr ')'                  # expr_parenthesis
    | expr '.' NAME                 # expr_member
    | expr '[' expr ']'             # expr_subscript
    // | function_expr                 # expr_func
    | NAME '(' argumentlist ')'             # expr_func_no_caller
    | expr '.' NAME '(' argumentlist ')'    # expr_func_with_caller
    | operatorUnary expr            # expr_unary
    | expr operatorMulDivMod expr   # expr_mul_div
    | expr operatorAddSub expr      # expr_add_sub
    | expr operatorComparison expr  # expr_compare
    | expr operatorStrcat expr      # expr_strcat
    | expr operatorAnd expr	        # expr_and
    | expr operatorOr expr          # expr_or
    | expr operatorBitwise expr     # expr_bitwise
    ;

atom_expr
    : NUMBER                        # atom_number
    | STRING                        # atom_string
    | VARIABLE                      # atom_variable
    | TRUE                          # atom_true
    | FALSE                         # atom_false
    | NIL                           # atom_nil
    | SELF                          # atom_self
    ;

container_expr
    : '[' argumentlist ']'
    | '{' argumentlist '}'
    ;

assign_stat
    : VARIABLE '=' expr             # expr_var_assign
    | expr '.' NAME '=' expr          # expr_member_assign
    | expr '[' expr ']' '=' expr    # expr_subscript_assign
    ;



argument
    : expr                          # expr_arg
    | NAME '=' expr                 # expr_named_arg
    ;

argumentlist
	: (argument)? (',' argument)*
	;

// operator
operatorUnary
    : 'not' | '#' | '-' | '~';

operatorMulDivMod
	: '^' | MUL | DIV | '%';

operatorAddSub
	: ADD | SUB;

operatorStrcat
	: '..';

// bitwise operator should have different precedence
operatorBitwise
	: '&' | '|' | '~' | '<<' | '>>';

operatorComparison 
	: '<' | '>' | '<=' | '>=' | '~=' | '==' | '!=';

operatorAnd 
	: 'and' | '&&';

operatorOr 
	: 'or' | '||';