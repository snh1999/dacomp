- Parser tokens
    - reserved words
    - operators
        - uniary
        - binary
    - others
        - identifier : variables
        - integer/number literal
        - string literal
        - EOF

Tokenization
Grammer Specification
Expression
    - operator precedence
    - binary expression
    - ternary
Statement - whatever makes statement
    Terminal operator
    Non-terminal operator



Steps:
    - input : stream of characters
1. Lexical Analysis (Tokenizer/Lexer using Finite Automata)
    - stream of token
            - Tokenization
                - Identifier : variables
                - Separators : allowed symbols
                - keywords/ reserved words
                - Operator : Logical/Arithmetic
                - Constant : value
                - Special Character
            - Pick Lexical Errors
                - Unmatched Separator
                - Illegal String close (string counts as one token)
                - Exceeding Length
            - Remove comment, space
2. Syntax Analysis (Parser- checks grammer using Context Free Diagram)
    - Parse Tree

- **First**
first(terminal/E) = terminal/E
first(variable) = first_of_first_variable 
    (if E, put E in the variable)
first(value/terminal) = first_value_of_terminal ('|' becomes comma separated)
- **Follow**
follow(start) = $
follow(variable) = first(next_var) | terminal (for last variable, same as Left variable)
    follow do not have E

3. Semantic Analysis (logical error, scoping, declaration)
    - Syntax Directory Translation
4. Intermediate Code Generator
    - 3 Address code
5. Code Optimization
6. Target(Machine) Code Generation
    

