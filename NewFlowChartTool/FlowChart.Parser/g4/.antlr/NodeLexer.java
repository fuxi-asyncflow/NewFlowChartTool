// Generated from f:\asyncflow\NewFlowChartTool\NewFlowChartTool\FlowChart.Parser\g4\NodeLexer.g4 by ANTLR 4.9.2
import org.antlr.v4.runtime.Lexer;
import org.antlr.v4.runtime.CharStream;
import org.antlr.v4.runtime.Token;
import org.antlr.v4.runtime.TokenStream;
import org.antlr.v4.runtime.*;
import org.antlr.v4.runtime.atn.*;
import org.antlr.v4.runtime.dfa.DFA;
import org.antlr.v4.runtime.misc.*;

@SuppressWarnings({"all", "warnings", "unchecked", "unused", "cast"})
public class NodeLexer extends Lexer {
	static { RuntimeMetaData.checkVersion("4.9.2", RuntimeMetaData.VERSION); }

	protected static final DFA[] _decisionToDFA;
	protected static final PredictionContextCache _sharedContextCache =
		new PredictionContextCache();
	public static final int
		TRUE=1, FALSE=2, NIL=3, SELF=4, ASSIGN=5, GT=6, LT=7, BANG=8, TILDE=9, 
		QUESTION=10, COLON=11, EQUAL=12, LE=13, GE=14, NOTEQUAL=15, AND=16, OR=17, 
		ADD=18, SUB=19, MUL=20, DIV=21, NAME=22, VARIABLE=23, STRING=24, NUMBER=25, 
		DEC_INTEGER=26, HEX_INTEGER=27, FLOAT_NUMBER=28;
	public static String[] channelNames = {
		"DEFAULT_TOKEN_CHANNEL", "HIDDEN"
	};

	public static String[] modeNames = {
		"DEFAULT_MODE"
	};

	private static String[] makeRuleNames() {
		return new String[] {
			"TRUE", "FALSE", "NIL", "SELF", "ASSIGN", "GT", "LT", "BANG", "TILDE", 
			"QUESTION", "COLON", "EQUAL", "LE", "GE", "NOTEQUAL", "AND", "OR", "ADD", 
			"SUB", "MUL", "DIV", "NAME", "VARIABLE", "STRING", "STRING_ESCAPE_SEQ", 
			"NUMBER", "DEC_INTEGER", "HEX_INTEGER", "FLOAT_NUMBER", "DIGIT", "HEX_DIGIT", 
			"POINT_FLOAT", "EXPONENT_FLOAT", "INT_PART", "FRACTION", "EXPONENT"
		};
	}
	public static final String[] ruleNames = makeRuleNames();

	private static String[] makeLiteralNames() {
		return new String[] {
			null, "'true'", "'false'", "'nil'", "'self'", "'='", "'>'", "'<'", "'!'", 
			"'~'", "'?'", "':'", "'=='", "'<='", "'>='", null, null, null, "'+'", 
			"'-'", "'*'", "'/'"
		};
	}
	private static final String[] _LITERAL_NAMES = makeLiteralNames();
	private static String[] makeSymbolicNames() {
		return new String[] {
			null, "TRUE", "FALSE", "NIL", "SELF", "ASSIGN", "GT", "LT", "BANG", "TILDE", 
			"QUESTION", "COLON", "EQUAL", "LE", "GE", "NOTEQUAL", "AND", "OR", "ADD", 
			"SUB", "MUL", "DIV", "NAME", "VARIABLE", "STRING", "NUMBER", "DEC_INTEGER", 
			"HEX_INTEGER", "FLOAT_NUMBER"
		};
	}
	private static final String[] _SYMBOLIC_NAMES = makeSymbolicNames();
	public static final Vocabulary VOCABULARY = new VocabularyImpl(_LITERAL_NAMES, _SYMBOLIC_NAMES);

	/**
	 * @deprecated Use {@link #VOCABULARY} instead.
	 */
	@Deprecated
	public static final String[] tokenNames;
	static {
		tokenNames = new String[_SYMBOLIC_NAMES.length];
		for (int i = 0; i < tokenNames.length; i++) {
			tokenNames[i] = VOCABULARY.getLiteralName(i);
			if (tokenNames[i] == null) {
				tokenNames[i] = VOCABULARY.getSymbolicName(i);
			}

			if (tokenNames[i] == null) {
				tokenNames[i] = "<INVALID>";
			}
		}
	}

	@Override
	@Deprecated
	public String[] getTokenNames() {
		return tokenNames;
	}

	@Override

	public Vocabulary getVocabulary() {
		return VOCABULARY;
	}


	public NodeLexer(CharStream input) {
		super(input);
		_interp = new LexerATNSimulator(this,_ATN,_decisionToDFA,_sharedContextCache);
	}

	@Override
	public String getGrammarFileName() { return "NodeLexer.g4"; }

	@Override
	public String[] getRuleNames() { return ruleNames; }

	@Override
	public String getSerializedATN() { return _serializedATN; }

	@Override
	public String[] getChannelNames() { return channelNames; }

	@Override
	public String[] getModeNames() { return modeNames; }

	@Override
	public ATN getATN() { return _ATN; }

	public static final String _serializedATN =
		"\3\u608b\ua72a\u8133\ub9ed\u417c\u3be7\u7786\u5964\2\36\u00ef\b\1\4\2"+
		"\t\2\4\3\t\3\4\4\t\4\4\5\t\5\4\6\t\6\4\7\t\7\4\b\t\b\4\t\t\t\4\n\t\n\4"+
		"\13\t\13\4\f\t\f\4\r\t\r\4\16\t\16\4\17\t\17\4\20\t\20\4\21\t\21\4\22"+
		"\t\22\4\23\t\23\4\24\t\24\4\25\t\25\4\26\t\26\4\27\t\27\4\30\t\30\4\31"+
		"\t\31\4\32\t\32\4\33\t\33\4\34\t\34\4\35\t\35\4\36\t\36\4\37\t\37\4 \t"+
		" \4!\t!\4\"\t\"\4#\t#\4$\t$\4%\t%\3\2\3\2\3\2\3\2\3\2\3\3\3\3\3\3\3\3"+
		"\3\3\3\3\3\4\3\4\3\4\3\4\3\5\3\5\3\5\3\5\3\5\3\6\3\6\3\7\3\7\3\b\3\b\3"+
		"\t\3\t\3\n\3\n\3\13\3\13\3\f\3\f\3\r\3\r\3\r\3\16\3\16\3\16\3\17\3\17"+
		"\3\17\3\20\3\20\3\20\3\20\3\20\5\20|\n\20\3\21\3\21\3\21\3\21\3\21\5\21"+
		"\u0083\n\21\3\22\3\22\3\22\3\22\5\22\u0089\n\22\3\23\3\23\3\24\3\24\3"+
		"\25\3\25\3\26\3\26\3\27\3\27\7\27\u0095\n\27\f\27\16\27\u0098\13\27\3"+
		"\30\3\30\3\30\3\31\3\31\3\31\7\31\u00a0\n\31\f\31\16\31\u00a3\13\31\3"+
		"\31\3\31\3\31\3\31\7\31\u00a9\n\31\f\31\16\31\u00ac\13\31\3\31\5\31\u00af"+
		"\n\31\3\32\3\32\3\32\3\33\3\33\3\33\5\33\u00b7\n\33\3\34\6\34\u00ba\n"+
		"\34\r\34\16\34\u00bb\3\35\3\35\3\35\6\35\u00c1\n\35\r\35\16\35\u00c2\3"+
		"\36\3\36\5\36\u00c7\n\36\3\37\3\37\3 \3 \3!\5!\u00ce\n!\3!\3!\3!\3!\5"+
		"!\u00d4\n!\3\"\3\"\5\"\u00d8\n\"\3\"\3\"\3#\6#\u00dd\n#\r#\16#\u00de\3"+
		"$\3$\6$\u00e3\n$\r$\16$\u00e4\3%\3%\5%\u00e9\n%\3%\6%\u00ec\n%\r%\16%"+
		"\u00ed\2\2&\3\3\5\4\7\5\t\6\13\7\r\b\17\t\21\n\23\13\25\f\27\r\31\16\33"+
		"\17\35\20\37\21!\22#\23%\24\'\25)\26+\27-\30/\31\61\32\63\2\65\33\67\34"+
		"9\35;\36=\2?\2A\2C\2E\2G\2I\2\3\2\13\5\2C\\aac|\6\2\62;C\\aac|\6\2\f\f"+
		"\16\17))^^\6\2\f\f\16\17$$^^\4\2ZZzz\3\2\62;\5\2\62;CHch\4\2GGgg\4\2-"+
		"-//\2\u00fb\2\3\3\2\2\2\2\5\3\2\2\2\2\7\3\2\2\2\2\t\3\2\2\2\2\13\3\2\2"+
		"\2\2\r\3\2\2\2\2\17\3\2\2\2\2\21\3\2\2\2\2\23\3\2\2\2\2\25\3\2\2\2\2\27"+
		"\3\2\2\2\2\31\3\2\2\2\2\33\3\2\2\2\2\35\3\2\2\2\2\37\3\2\2\2\2!\3\2\2"+
		"\2\2#\3\2\2\2\2%\3\2\2\2\2\'\3\2\2\2\2)\3\2\2\2\2+\3\2\2\2\2-\3\2\2\2"+
		"\2/\3\2\2\2\2\61\3\2\2\2\2\65\3\2\2\2\2\67\3\2\2\2\29\3\2\2\2\2;\3\2\2"+
		"\2\3K\3\2\2\2\5P\3\2\2\2\7V\3\2\2\2\tZ\3\2\2\2\13_\3\2\2\2\ra\3\2\2\2"+
		"\17c\3\2\2\2\21e\3\2\2\2\23g\3\2\2\2\25i\3\2\2\2\27k\3\2\2\2\31m\3\2\2"+
		"\2\33p\3\2\2\2\35s\3\2\2\2\37{\3\2\2\2!\u0082\3\2\2\2#\u0088\3\2\2\2%"+
		"\u008a\3\2\2\2\'\u008c\3\2\2\2)\u008e\3\2\2\2+\u0090\3\2\2\2-\u0092\3"+
		"\2\2\2/\u0099\3\2\2\2\61\u00ae\3\2\2\2\63\u00b0\3\2\2\2\65\u00b6\3\2\2"+
		"\2\67\u00b9\3\2\2\29\u00bd\3\2\2\2;\u00c6\3\2\2\2=\u00c8\3\2\2\2?\u00ca"+
		"\3\2\2\2A\u00d3\3\2\2\2C\u00d7\3\2\2\2E\u00dc\3\2\2\2G\u00e0\3\2\2\2I"+
		"\u00e6\3\2\2\2KL\7v\2\2LM\7t\2\2MN\7w\2\2NO\7g\2\2O\4\3\2\2\2PQ\7h\2\2"+
		"QR\7c\2\2RS\7n\2\2ST\7u\2\2TU\7g\2\2U\6\3\2\2\2VW\7p\2\2WX\7k\2\2XY\7"+
		"n\2\2Y\b\3\2\2\2Z[\7u\2\2[\\\7g\2\2\\]\7n\2\2]^\7h\2\2^\n\3\2\2\2_`\7"+
		"?\2\2`\f\3\2\2\2ab\7@\2\2b\16\3\2\2\2cd\7>\2\2d\20\3\2\2\2ef\7#\2\2f\22"+
		"\3\2\2\2gh\7\u0080\2\2h\24\3\2\2\2ij\7A\2\2j\26\3\2\2\2kl\7<\2\2l\30\3"+
		"\2\2\2mn\7?\2\2no\7?\2\2o\32\3\2\2\2pq\7>\2\2qr\7?\2\2r\34\3\2\2\2st\7"+
		"@\2\2tu\7?\2\2u\36\3\2\2\2vw\7#\2\2w|\7?\2\2xy\7p\2\2yz\7q\2\2z|\7v\2"+
		"\2{v\3\2\2\2{x\3\2\2\2| \3\2\2\2}~\7(\2\2~\u0083\7(\2\2\177\u0080\7c\2"+
		"\2\u0080\u0081\7p\2\2\u0081\u0083\7f\2\2\u0082}\3\2\2\2\u0082\177\3\2"+
		"\2\2\u0083\"\3\2\2\2\u0084\u0085\7~\2\2\u0085\u0089\7~\2\2\u0086\u0087"+
		"\7q\2\2\u0087\u0089\7t\2\2\u0088\u0084\3\2\2\2\u0088\u0086\3\2\2\2\u0089"+
		"$\3\2\2\2\u008a\u008b\7-\2\2\u008b&\3\2\2\2\u008c\u008d\7/\2\2\u008d("+
		"\3\2\2\2\u008e\u008f\7,\2\2\u008f*\3\2\2\2\u0090\u0091\7\61\2\2\u0091"+
		",\3\2\2\2\u0092\u0096\t\2\2\2\u0093\u0095\t\3\2\2\u0094\u0093\3\2\2\2"+
		"\u0095\u0098\3\2\2\2\u0096\u0094\3\2\2\2\u0096\u0097\3\2\2\2\u0097.\3"+
		"\2\2\2\u0098\u0096\3\2\2\2\u0099\u009a\7&\2\2\u009a\u009b\5-\27\2\u009b"+
		"\60\3\2\2\2\u009c\u00a1\7)\2\2\u009d\u00a0\5\63\32\2\u009e\u00a0\n\4\2"+
		"\2\u009f\u009d\3\2\2\2\u009f\u009e\3\2\2\2\u00a0\u00a3\3\2\2\2\u00a1\u009f"+
		"\3\2\2\2\u00a1\u00a2\3\2\2\2\u00a2\u00a4\3\2\2\2\u00a3\u00a1\3\2\2\2\u00a4"+
		"\u00af\7)\2\2\u00a5\u00aa\7$\2\2\u00a6\u00a9\5\63\32\2\u00a7\u00a9\n\5"+
		"\2\2\u00a8\u00a6\3\2\2\2\u00a8\u00a7\3\2\2\2\u00a9\u00ac\3\2\2\2\u00aa"+
		"\u00a8\3\2\2\2\u00aa\u00ab\3\2\2\2\u00ab\u00ad\3\2\2\2\u00ac\u00aa\3\2"+
		"\2\2\u00ad\u00af\7$\2\2\u00ae\u009c\3\2\2\2\u00ae\u00a5\3\2\2\2\u00af"+
		"\62\3\2\2\2\u00b0\u00b1\7^\2\2\u00b1\u00b2\13\2\2\2\u00b2\64\3\2\2\2\u00b3"+
		"\u00b7\5\67\34\2\u00b4\u00b7\59\35\2\u00b5\u00b7\5;\36\2\u00b6\u00b3\3"+
		"\2\2\2\u00b6\u00b4\3\2\2\2\u00b6\u00b5\3\2\2\2\u00b7\66\3\2\2\2\u00b8"+
		"\u00ba\5=\37\2\u00b9\u00b8\3\2\2\2\u00ba\u00bb\3\2\2\2\u00bb\u00b9\3\2"+
		"\2\2\u00bb\u00bc\3\2\2\2\u00bc8\3\2\2\2\u00bd\u00be\7\62\2\2\u00be\u00c0"+
		"\t\6\2\2\u00bf\u00c1\5? \2\u00c0\u00bf\3\2\2\2\u00c1\u00c2\3\2\2\2\u00c2"+
		"\u00c0\3\2\2\2\u00c2\u00c3\3\2\2\2\u00c3:\3\2\2\2\u00c4\u00c7\5A!\2\u00c5"+
		"\u00c7\5C\"\2\u00c6\u00c4\3\2\2\2\u00c6\u00c5\3\2\2\2\u00c7<\3\2\2\2\u00c8"+
		"\u00c9\t\7\2\2\u00c9>\3\2\2\2\u00ca\u00cb\t\b\2\2\u00cb@\3\2\2\2\u00cc"+
		"\u00ce\5E#\2\u00cd\u00cc\3\2\2\2\u00cd\u00ce\3\2\2\2\u00ce\u00cf\3\2\2"+
		"\2\u00cf\u00d4\5G$\2\u00d0\u00d1\5E#\2\u00d1\u00d2\7\60\2\2\u00d2\u00d4"+
		"\3\2\2\2\u00d3\u00cd\3\2\2\2\u00d3\u00d0\3\2\2\2\u00d4B\3\2\2\2\u00d5"+
		"\u00d8\5E#\2\u00d6\u00d8\5A!\2\u00d7\u00d5\3\2\2\2\u00d7\u00d6\3\2\2\2"+
		"\u00d8\u00d9\3\2\2\2\u00d9\u00da\5I%\2\u00daD\3\2\2\2\u00db\u00dd\5=\37"+
		"\2\u00dc\u00db\3\2\2\2\u00dd\u00de\3\2\2\2\u00de\u00dc\3\2\2\2\u00de\u00df"+
		"\3\2\2\2\u00dfF\3\2\2\2\u00e0\u00e2\7\60\2\2\u00e1\u00e3\5=\37\2\u00e2"+
		"\u00e1\3\2\2\2\u00e3\u00e4\3\2\2\2\u00e4\u00e2\3\2\2\2\u00e4\u00e5\3\2"+
		"\2\2\u00e5H\3\2\2\2\u00e6\u00e8\t\t\2\2\u00e7\u00e9\t\n\2\2\u00e8\u00e7"+
		"\3\2\2\2\u00e8\u00e9\3\2\2\2\u00e9\u00eb\3\2\2\2\u00ea\u00ec\5=\37\2\u00eb"+
		"\u00ea\3\2\2\2\u00ec\u00ed\3\2\2\2\u00ed\u00eb\3\2\2\2\u00ed\u00ee\3\2"+
		"\2\2\u00eeJ\3\2\2\2\27\2{\u0082\u0088\u0096\u009f\u00a1\u00a8\u00aa\u00ae"+
		"\u00b6\u00bb\u00c2\u00c6\u00cd\u00d3\u00d7\u00de\u00e4\u00e8\u00ed\2";
	public static final ATN _ATN =
		new ATNDeserializer().deserialize(_serializedATN.toCharArray());
	static {
		_decisionToDFA = new DFA[_ATN.getNumberOfDecisions()];
		for (int i = 0; i < _ATN.getNumberOfDecisions(); i++) {
			_decisionToDFA[i] = new DFA(_ATN.getDecisionState(i), i);
		}
	}
}