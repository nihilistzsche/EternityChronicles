//
// ECX-dep.g
//
// Author:
//     Michael Tindal <urilith@gentoo.org>
//
// Copyright (C) 2005 Michael Tindal and the individuals listed on
// the ChangeLog entries.
//
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
// 
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//

grammar ECX;
 
options
{
    language=CSharp3;
    backtrack=true;
}

@header {
	//
// ANTLR Generated Files.
//
// Author:
//     Michael Tindal <urilith@gentoo.org>
//
// Copyright (C) 2005 Michael Tindal and the individuals listed on
// the ChangeLog entries.
//
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
// 
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//

using ECX.Core.Dependency;

	// Class does not need CLSCompliant attribute
	#pragma warning disable 3021
}

@namespace { ECX.Core.Dependency.Parser }

// Parentheses
LPAREN: '(' ;
RPAREN: ')' ;

// Combination Operators
AND: '&&' ;
OR: '||' ;
XOR: '^^' ;
OPT: '??' ;

// Dependency Operators
EQ: '==' ;
NEQ: '!=' ;
LTE: '<=' ;
LT: '<<' ;
GTE: '>=' ;
GT: '>>' ;
LD: '##' ;
NL: '!#' ;

// Version
fragment
INT: ('0' .. '9')+;

VER: INT (DOT INT (DOT INT (DOT INT)?)?)?;

// Dot operator
fragment
DOT: '.' ;

// Basic Identifier
fragment
ID_START_LETTER 
	: ('a' .. 'z')
	| ('A' .. 'Z')
	| ('_')
	;

fragment
ID_LETTER
	: ID_START_LETTER
	| ('0' .. '9')
	| ('-')
	;
	
fragment
ID: ID_START_LETTER ( ID_LETTER )* ;

// Class
CLASS: ID ( DOT ID )* ;


WS  :   ( ' '
        | '\t'
        | '\r'
        | '\n'
        ) {$channel=Hidden;}
    ;

// parser rules
public expr returns [DepNode result]
	: zexpr EOF { $result = $zexpr.result; }
	;
	
cexpr returns [DepNode result]
	@init{DepOps op = DepOps.Null; List<DepNode> zn = new List<DepNode>(); }
	: LPAREN (AND { op = DepOps.And; } | OR { op = DepOps.Or;} | XOR { op = DepOps.Xor; } | OPT { op = DepOps.Opt; }) (zexpr { zn.Add($zexpr.result); })+ RPAREN {
		var dn = new DepNode(op);
		zn.ForEach(node => dn.AddChild(node));
		$result = dn;
	}
	;


zexpr returns [DepNode result]
	: cexpr { $result = $cexpr.result; } | oexpr { $result = $oexpr.result; }
	;

oexpr returns [DepNode result]
	@init { DepOps op = DepOps.Null; }
	: LPAREN (EQ { op = DepOps.Equal; } | NEQ { op = DepOps.NotEqual; } | LTE { op = DepOps.LessThanEqual; } | LT { op = DepOps.LessThan; }
	| GTE { op = DepOps.GreaterThanEqual; } | GT { op = DepOps.GreaterThan; } | LD { op = DepOps.Loaded; } | NL { op = DepOps.NotLoaded; })
	iexpr RPAREN {
		$result = new DepNode(op, $iexpr.result);
	}
	;


iexpr returns [DepConstraint result]
	@init{ var con = new DepConstraint(); }
	: c=CLASS ( v=VER { con.SetVersion($v.Text); })? {
		con.Name = $c.Text;
		$result = con;
	}
	;
