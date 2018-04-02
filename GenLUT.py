# Taken straight from lj_char.h
enums = {
	'CNTRL': 0x01,
	'SPACE': 0x02,
	'PUNCT': 0x04,
	'DIGIT': 0x08,
	'XDIGIT': 0x10,
	'UPPER': 0x20,
	'LOWER': 0x40,
	'IDENT': 0x80
	# rest ins't included because they are
	# compositions of the other enums therefore
	# they can pose a problem
}

# Taken straight from lj_char.c
lut = [
	1,  1,  1,  1,  1,  1,  1,  1,  1,  3,  3,  3,  3,  3,  1,  1,
	1,  1,  1,  1,  1,  1,  1,  1,  1,  1,  1,  1,  1,  1,  1,  1,
	2,  4,  4,  4,  4,  4,  4,  4,  4,  4,  4,  4,  4,  4,  4,  4,
  152,152,152,152,152,152,152,152,152,152,  4,  4,  4,  4,  4,  4,
	4,176,176,176,176,176,176,160,160,160,160,160,160,160,160,160,
  160,160,160,160,160,160,160,160,160,160,160,  4,  4,  4,  4,132,
	4,208,208,208,208,208,208,192,192,192,192,192,192,192,192,192,
  192,192,192,192,192,192,192,192,192,192,192,  4,  4,  4,  4,  1,
  128,128,128,128,128,128,128,128,128,128,128,128,128,128,128,128,
  128,128,128,128,128,128,128,128,128,128,128,128,128,128,128,128,
  128,128,128,128,128,128,128,128,128,128,128,128,128,128,128,128,
  128,128,128,128,128,128,128,128,128,128,128,128,128,128,128,128,
  128,128,128,128,128,128,128,128,128,128,128,128,128,128,128,128,
  128,128,128,128,128,128,128,128,128,128,128,128,128,128,128,128,
  128,128,128,128,128,128,128,128,128,128,128,128,128,128,128,128,
  128,128,128,128,128,128,128,128,128,128,128,128,128,128,128,128
]

with open ( 'list.cs', 'w' ) as outp:
	outp.write ( '\t\tpublic static readonly LJ_CHAR[] CharList = new LJ_CHAR[] {\n\t\t\t#region Long ass (generated) char list\n' )
	for i, val in enumerate ( lut ):
		enumList = []
		for name in enums:
			if ( val & enums[name] ) != 0:
				enumList.append ( 'LJ_CHAR.' + name );

		outp.write ( ( '\t\t\t/* %6s(0x%2X) */ ' % ( repr ( chr ( i ) ), i ) ) )
		outp.write ( ' | '.join ( enumList ) )
		outp.write ( ',\n' if i != ( len ( lut ) - 1 ) else '\n' )
	outp.write ( '\t\t\t#endregion\n\t\t};' )