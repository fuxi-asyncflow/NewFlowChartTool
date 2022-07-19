// Generated from f:\asyncflow\NewFlowChartTool\NewFlowChartTool\FlowChart.Parser\g4\NodeParser.g4 by ANTLR 4.9.2
import org.antlr.v4.runtime.Lexer;
import org.antlr.v4.runtime.CharStream;
import org.antlr.v4.runtime.Token;
import org.antlr.v4.runtime.TokenStream;
import org.antlr.v4.runtime.*;
import org.antlr.v4.runtime.atn.*;
import org.antlr.v4.runtime.dfa.DFA;
import org.antlr.v4.runtime.misc.*;

@SuppressWarnings({"all", "warnings", "unchecked", "unused", "cast"})
public class NodeParserLexer extends Lexer {
	static { RuntimeMetaData.checkVersion("4.9.2", RuntimeMetaData.VERSION); }

	protected static final DFA[] _decisionToDFA;
	protected static final PredictionContextCache _sharedContextCache =
		new PredictionContextCache();
	public static final int
		T__0=1, T__1=2, T__2=3, T__3=4, T__4=5, T__5=6, T__6=7, T__7=8, T__8=9, 
		T__9=10, T__10=11, T__11=12, T__12=13, T__13=14, T__14=15, T__15=16, T__16=17, 
		T__17=18, T__18=19, T__19=20, T__20=21, T__21=22, T__22=23, TRUE=24, FALSE=25, 
		NIL=26, SELF=27, ASSIGN=28, GT=29, LT=30, BANG=31, TILDE=32, QUESTION=33, 
		COLON=34, EQUAL=35, LE=36, GE=37, NOTEQUAL=38, AND=39, OR=40, ADD=41, 
		SUB=42, MUL=43, DIV=44, NAME=45, VARIABLE=46, STRING=47, NUMBER=48, DEC_INTEGER=49, 
		HEX_INTEGER=50, FLOAT_NUMBER=51;
	public static String[] channelNames = {
		"DEFAULT_TOKEN_CHANNEL", "HIDDEN"
	};

	public static String[] modeNames = {
		"DEFAULT_MODE"
	};

	private static String[] makeRuleNames() {
		return new String[] {
			"T__0", "T__1", "T__2", "T__3", "T__4", "T__5", "T__6", "T__7", "T__8", 
			"T__9", "T__10", "T__11", "T__12", "T__13", "T__14", "T__15", "T__16", 
			"T__17", "T__18", "T__19", "T__20", "T__21", "T__22", "TRUE", "FALSE", 
			"NIL", "SELF", "ASSIGN", "GT", "LT", "BANG", "TILDE", "QUESTION", "COLON", 
			"EQUAL", "LE", "GE", "NOTEQUAL", "AND", "OR", "ADD", "SUB", "MUL", "DIV", 
			"NAME", "VARIABLE", "STRING", "STRING_ESCAPE_SEQ", "NUMBER", "DEC_INTEGER", 
			"HEX_INTEGER", "FLOAT_NUMBER", "DIGIT", "HEX_DIGIT", "POINT_FLOAT", "EXPONENT_FLOAT", 
			"INT_PART", "FRACTION", "EXPONENT"
		};
	}
	public static final String[] ruleNames = makeRuleNames();

	private static String[] makeLiteralNames() {
		return new String[] {
			null, "'('", "')'", "'.'", "'['", "']'", "'{'", "'}'", "','", "'not'", 
			"'#'", "'^'", "'%'", "'..'", "'&'", "'|'", "'<<'", "'>>'", "'~='", "'!='", 
			"'and'", "'&&'", "'or'", "'||'", "'true'", "'false'", "'nil'", "'self'", 
			"'='", "'>'", "'<'", "'!'", "'~'", "'?'", "':'", "'=='", "'<='", "'>='", 
			null, null, null, "'+'", "'-'", "'*'", "'/'"
		};
	}
	private static final String[] _LITERAL_NAMES = makeLiteralNames();
	private static String[] makeSymbolicNames() {
		return new String[] {
			null, null, null, null, null, null, null, null, null, null, null, null, 
			null, null, null, null, null, null, null, null, null, null, null, null, 
			"TRUE", "FALSE", "NIL", "SELF", "ASSIGN", "GT", "LT", "BANG", "TILDE", 
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


	public NodeParserLexer(CharStream input) {
		super(input);
		_interp = new LexerATNSimulator(this,_ATN,_decisionToDFA,_sharedContextCache);
	}

	@Override
	public String getGrammarFileName() { return "NodeParser.g4"; }

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
		"\3\u608b\ua72a\u8133\ub9ed\u417c\u3be7\u7786\u5964\2\65\u0157\b\1\4\2"+
		"\t\2\4\3\t\3\4\4\t\4\4\5\t\5\4\6\t\6\4\7\t\7\4\b\t\b\4\t\t\t\4\n\t\n\4"+
		"\13\t\13\4\f\t\f\4\r\t\r\4\16\t\16\4\17\t\17\4\20\t\20\4\21\t\21\4\22"+
		"\t\22\4\23\t\23\4\24\t\24\4\25\t\25\4\26\t\26\4\27\t\27\4\30\t\30\4\31"+
		"\t\31\4\32\t\32\4\33\t\33\4\34\t\34\4\35\t\35\4\36\t\36\4\37\t\37\4 \t"+
		" \4!\t!\4\"\t\"\4#\t#\4$\t$\4%\t%\4&\t&\4\'\t\'\4(\t(\4)\t)\4*\t*\4+\t"+
		"+\4,\t,\4-\t-\4.\t.\4/\t/\4\60\t\60\4\61\t\61\4\62\t\62\4\63\t\63\4\64"+
		"\t\64\4\65\t\65\4\66\t\66\4\67\t\67\48\t8\49\t9\4:\t:\4;\t;\4<\t<\3\2"+
		"\3\2\3\3\3\3\3\4\3\4\3\5\3\5\3\6\3\6\3\7\3\7\3\b\3\b\3\t\3\t\3\n\3\n\3"+
		"\n\3\n\3\13\3\13\3\f\3\f\3\r\3\r\3\16\3\16\3\16\3\17\3\17\3\20\3\20\3"+
		"\21\3\21\3\21\3\22\3\22\3\22\3\23\3\23\3\23\3\24\3\24\3\24\3\25\3\25\3"+
		"\25\3\25\3\26\3\26\3\26\3\27\3\27\3\27\3\30\3\30\3\30\3\31\3\31\3\31\3"+
		"\31\3\31\3\32\3\32\3\32\3\32\3\32\3\32\3\33\3\33\3\33\3\33\3\34\3\34\3"+
		"\34\3\34\3\34\3\35\3\35\3\36\3\36\3\37\3\37\3 \3 \3!\3!\3\"\3\"\3#\3#"+
		"\3$\3$\3$\3%\3%\3%\3&\3&\3&\3\'\3\'\3\'\3\'\3\'\5\'\u00e4\n\'\3(\3(\3"+
		"(\3(\3(\5(\u00eb\n(\3)\3)\3)\3)\5)\u00f1\n)\3*\3*\3+\3+\3,\3,\3-\3-\3"+
		".\3.\7.\u00fd\n.\f.\16.\u0100\13.\3/\3/\3/\3\60\3\60\3\60\7\60\u0108\n"+
		"\60\f\60\16\60\u010b\13\60\3\60\3\60\3\60\3\60\7\60\u0111\n\60\f\60\16"+
		"\60\u0114\13\60\3\60\5\60\u0117\n\60\3\61\3\61\3\61\3\62\3\62\3\62\5\62"+
		"\u011f\n\62\3\63\6\63\u0122\n\63\r\63\16\63\u0123\3\64\3\64\3\64\6\64"+
		"\u0129\n\64\r\64\16\64\u012a\3\65\3\65\5\65\u012f\n\65\3\66\3\66\3\67"+
		"\3\67\38\58\u0136\n8\38\38\38\38\58\u013c\n8\39\39\59\u0140\n9\39\39\3"+
		":\6:\u0145\n:\r:\16:\u0146\3;\3;\6;\u014b\n;\r;\16;\u014c\3<\3<\5<\u0151"+
		"\n<\3<\6<\u0154\n<\r<\16<\u0155\2\2=\3\3\5\4\7\5\t\6\13\7\r\b\17\t\21"+
		"\n\23\13\25\f\27\r\31\16\33\17\35\20\37\21!\22#\23%\24\'\25)\26+\27-\30"+
		"/\31\61\32\63\33\65\34\67\359\36;\37= ?!A\"C#E$G%I&K\'M(O)Q*S+U,W-Y.["+
		"/]\60_\61a\2c\62e\63g\64i\65k\2m\2o\2q\2s\2u\2w\2\3\2\13\5\2C\\aac|\6"+
		"\2\62;C\\aac|\6\2\f\f\16\17))^^\6\2\f\f\16\17$$^^\4\2ZZzz\3\2\62;\5\2"+
		"\62;CHch\4\2GGgg\4\2--//\2\u0163\2\3\3\2\2\2\2\5\3\2\2\2\2\7\3\2\2\2\2"+
		"\t\3\2\2\2\2\13\3\2\2\2\2\r\3\2\2\2\2\17\3\2\2\2\2\21\3\2\2\2\2\23\3\2"+
		"\2\2\2\25\3\2\2\2\2\27\3\2\2\2\2\31\3\2\2\2\2\33\3\2\2\2\2\35\3\2\2\2"+
		"\2\37\3\2\2\2\2!\3\2\2\2\2#\3\2\2\2\2%\3\2\2\2\2\'\3\2\2\2\2)\3\2\2\2"+
		"\2+\3\2\2\2\2-\3\2\2\2\2/\3\2\2\2\2\61\3\2\2\2\2\63\3\2\2\2\2\65\3\2\2"+
		"\2\2\67\3\2\2\2\29\3\2\2\2\2;\3\2\2\2\2=\3\2\2\2\2?\3\2\2\2\2A\3\2\2\2"+
		"\2C\3\2\2\2\2E\3\2\2\2\2G\3\2\2\2\2I\3\2\2\2\2K\3\2\2\2\2M\3\2\2\2\2O"+
		"\3\2\2\2\2Q\3\2\2\2\2S\3\2\2\2\2U\3\2\2\2\2W\3\2\2\2\2Y\3\2\2\2\2[\3\2"+
		"\2\2\2]\3\2\2\2\2_\3\2\2\2\2c\3\2\2\2\2e\3\2\2\2\2g\3\2\2\2\2i\3\2\2\2"+
		"\3y\3\2\2\2\5{\3\2\2\2\7}\3\2\2\2\t\177\3\2\2\2\13\u0081\3\2\2\2\r\u0083"+
		"\3\2\2\2\17\u0085\3\2\2\2\21\u0087\3\2\2\2\23\u0089\3\2\2\2\25\u008d\3"+
		"\2\2\2\27\u008f\3\2\2\2\31\u0091\3\2\2\2\33\u0093\3\2\2\2\35\u0096\3\2"+
		"\2\2\37\u0098\3\2\2\2!\u009a\3\2\2\2#\u009d\3\2\2\2%\u00a0\3\2\2\2\'\u00a3"+
		"\3\2\2\2)\u00a6\3\2\2\2+\u00aa\3\2\2\2-\u00ad\3\2\2\2/\u00b0\3\2\2\2\61"+
		"\u00b3\3\2\2\2\63\u00b8\3\2\2\2\65\u00be\3\2\2\2\67\u00c2\3\2\2\29\u00c7"+
		"\3\2\2\2;\u00c9\3\2\2\2=\u00cb\3\2\2\2?\u00cd\3\2\2\2A\u00cf\3\2\2\2C"+
		"\u00d1\3\2\2\2E\u00d3\3\2\2\2G\u00d5\3\2\2\2I\u00d8\3\2\2\2K\u00db\3\2"+
		"\2\2M\u00e3\3\2\2\2O\u00ea\3\2\2\2Q\u00f0\3\2\2\2S\u00f2\3\2\2\2U\u00f4"+
		"\3\2\2\2W\u00f6\3\2\2\2Y\u00f8\3\2\2\2[\u00fa\3\2\2\2]\u0101\3\2\2\2_"+
		"\u0116\3\2\2\2a\u0118\3\2\2\2c\u011e\3\2\2\2e\u0121\3\2\2\2g\u0125\3\2"+
		"\2\2i\u012e\3\2\2\2k\u0130\3\2\2\2m\u0132\3\2\2\2o\u013b\3\2\2\2q\u013f"+
		"\3\2\2\2s\u0144\3\2\2\2u\u0148\3\2\2\2w\u014e\3\2\2\2yz\7*\2\2z\4\3\2"+
		"\2\2{|\7+\2\2|\6\3\2\2\2}~\7\60\2\2~\b\3\2\2\2\177\u0080\7]\2\2\u0080"+
		"\n\3\2\2\2\u0081\u0082\7_\2\2\u0082\f\3\2\2\2\u0083\u0084\7}\2\2\u0084"+
		"\16\3\2\2\2\u0085\u0086\7\177\2\2\u0086\20\3\2\2\2\u0087\u0088\7.\2\2"+
		"\u0088\22\3\2\2\2\u0089\u008a\7p\2\2\u008a\u008b\7q\2\2\u008b\u008c\7"+
		"v\2\2\u008c\24\3\2\2\2\u008d\u008e\7%\2\2\u008e\26\3\2\2\2\u008f\u0090"+
		"\7`\2\2\u0090\30\3\2\2\2\u0091\u0092\7\'\2\2\u0092\32\3\2\2\2\u0093\u0094"+
		"\7\60\2\2\u0094\u0095\7\60\2\2\u0095\34\3\2\2\2\u0096\u0097\7(\2\2\u0097"+
		"\36\3\2\2\2\u0098\u0099\7~\2\2\u0099 \3\2\2\2\u009a\u009b\7>\2\2\u009b"+
		"\u009c\7>\2\2\u009c\"\3\2\2\2\u009d\u009e\7@\2\2\u009e\u009f\7@\2\2\u009f"+
		"$\3\2\2\2\u00a0\u00a1\7\u0080\2\2\u00a1\u00a2\7?\2\2\u00a2&\3\2\2\2\u00a3"+
		"\u00a4\7#\2\2\u00a4\u00a5\7?\2\2\u00a5(\3\2\2\2\u00a6\u00a7\7c\2\2\u00a7"+
		"\u00a8\7p\2\2\u00a8\u00a9\7f\2\2\u00a9*\3\2\2\2\u00aa\u00ab\7(\2\2\u00ab"+
		"\u00ac\7(\2\2\u00ac,\3\2\2\2\u00ad\u00ae\7q\2\2\u00ae\u00af\7t\2\2\u00af"+
		".\3\2\2\2\u00b0\u00b1\7~\2\2\u00b1\u00b2\7~\2\2\u00b2\60\3\2\2\2\u00b3"+
		"\u00b4\7v\2\2\u00b4\u00b5\7t\2\2\u00b5\u00b6\7w\2\2\u00b6\u00b7\7g\2\2"+
		"\u00b7\62\3\2\2\2\u00b8\u00b9\7h\2\2\u00b9\u00ba\7c\2\2\u00ba\u00bb\7"+
		"n\2\2\u00bb\u00bc\7u\2\2\u00bc\u00bd\7g\2\2\u00bd\64\3\2\2\2\u00be\u00bf"+
		"\7p\2\2\u00bf\u00c0\7k\2\2\u00c0\u00c1\7n\2\2\u00c1\66\3\2\2\2\u00c2\u00c3"+
		"\7u\2\2\u00c3\u00c4\7g\2\2\u00c4\u00c5\7n\2\2\u00c5\u00c6\7h\2\2\u00c6"+
		"8\3\2\2\2\u00c7\u00c8\7?\2\2\u00c8:\3\2\2\2\u00c9\u00ca\7@\2\2\u00ca<"+
		"\3\2\2\2\u00cb\u00cc\7>\2\2\u00cc>\3\2\2\2\u00cd\u00ce\7#\2\2\u00ce@\3"+
		"\2\2\2\u00cf\u00d0\7\u0080\2\2\u00d0B\3\2\2\2\u00d1\u00d2\7A\2\2\u00d2"+
		"D\3\2\2\2\u00d3\u00d4\7<\2\2\u00d4F\3\2\2\2\u00d5\u00d6\7?\2\2\u00d6\u00d7"+
		"\7?\2\2\u00d7H\3\2\2\2\u00d8\u00d9\7>\2\2\u00d9\u00da\7?\2\2\u00daJ\3"+
		"\2\2\2\u00db\u00dc\7@\2\2\u00dc\u00dd\7?\2\2\u00ddL\3\2\2\2\u00de\u00df"+
		"\7#\2\2\u00df\u00e4\7?\2\2\u00e0\u00e1\7p\2\2\u00e1\u00e2\7q\2\2\u00e2"+
		"\u00e4\7v\2\2\u00e3\u00de\3\2\2\2\u00e3\u00e0\3\2\2\2\u00e4N\3\2\2\2\u00e5"+
		"\u00e6\7(\2\2\u00e6\u00eb\7(\2\2\u00e7\u00e8\7c\2\2\u00e8\u00e9\7p\2\2"+
		"\u00e9\u00eb\7f\2\2\u00ea\u00e5\3\2\2\2\u00ea\u00e7\3\2\2\2\u00ebP\3\2"+
		"\2\2\u00ec\u00ed\7~\2\2\u00ed\u00f1\7~\2\2\u00ee\u00ef\7q\2\2\u00ef\u00f1"+
		"\7t\2\2\u00f0\u00ec\3\2\2\2\u00f0\u00ee\3\2\2\2\u00f1R\3\2\2\2\u00f2\u00f3"+
		"\7-\2\2\u00f3T\3\2\2\2\u00f4\u00f5\7/\2\2\u00f5V\3\2\2\2\u00f6\u00f7\7"+
		",\2\2\u00f7X\3\2\2\2\u00f8\u00f9\7\61\2\2\u00f9Z\3\2\2\2\u00fa\u00fe\t"+
		"\2\2\2\u00fb\u00fd\t\3\2\2\u00fc\u00fb\3\2\2\2\u00fd\u0100\3\2\2\2\u00fe"+
		"\u00fc\3\2\2\2\u00fe\u00ff\3\2\2\2\u00ff\\\3\2\2\2\u0100\u00fe\3\2\2\2"+
		"\u0101\u0102\7%\2\2\u0102\u0103\5[.\2\u0103^\3\2\2\2\u0104\u0109\7)\2"+
		"\2\u0105\u0108\5a\61\2\u0106\u0108\n\4\2\2\u0107\u0105\3\2\2\2\u0107\u0106"+
		"\3\2\2\2\u0108\u010b\3\2\2\2\u0109\u0107\3\2\2\2\u0109\u010a\3\2\2\2\u010a"+
		"\u010c\3\2\2\2\u010b\u0109\3\2\2\2\u010c\u0117\7)\2\2\u010d\u0112\7$\2"+
		"\2\u010e\u0111\5a\61\2\u010f\u0111\n\5\2\2\u0110\u010e\3\2\2\2\u0110\u010f"+
		"\3\2\2\2\u0111\u0114\3\2\2\2\u0112\u0110\3\2\2\2\u0112\u0113\3\2\2\2\u0113"+
		"\u0115\3\2\2\2\u0114\u0112\3\2\2\2\u0115\u0117\7$\2\2\u0116\u0104\3\2"+
		"\2\2\u0116\u010d\3\2\2\2\u0117`\3\2\2\2\u0118\u0119\7^\2\2\u0119\u011a"+
		"\13\2\2\2\u011ab\3\2\2\2\u011b\u011f\5e\63\2\u011c\u011f\5g\64\2\u011d"+
		"\u011f\5i\65\2\u011e\u011b\3\2\2\2\u011e\u011c\3\2\2\2\u011e\u011d\3\2"+
		"\2\2\u011fd\3\2\2\2\u0120\u0122\5k\66\2\u0121\u0120\3\2\2\2\u0122\u0123"+
		"\3\2\2\2\u0123\u0121\3\2\2\2\u0123\u0124\3\2\2\2\u0124f\3\2\2\2\u0125"+
		"\u0126\7\62\2\2\u0126\u0128\t\6\2\2\u0127\u0129\5m\67\2\u0128\u0127\3"+
		"\2\2\2\u0129\u012a\3\2\2\2\u012a\u0128\3\2\2\2\u012a\u012b\3\2\2\2\u012b"+
		"h\3\2\2\2\u012c\u012f\5o8\2\u012d\u012f\5q9\2\u012e\u012c\3\2\2\2\u012e"+
		"\u012d\3\2\2\2\u012fj\3\2\2\2\u0130\u0131\t\7\2\2\u0131l\3\2\2\2\u0132"+
		"\u0133\t\b\2\2\u0133n\3\2\2\2\u0134\u0136\5s:\2\u0135\u0134\3\2\2\2\u0135"+
		"\u0136\3\2\2\2\u0136\u0137\3\2\2\2\u0137\u013c\5u;\2\u0138\u0139\5s:\2"+
		"\u0139\u013a\7\60\2\2\u013a\u013c\3\2\2\2\u013b\u0135\3\2\2\2\u013b\u0138"+
		"\3\2\2\2\u013cp\3\2\2\2\u013d\u0140\5s:\2\u013e\u0140\5o8\2\u013f\u013d"+
		"\3\2\2\2\u013f\u013e\3\2\2\2\u0140\u0141\3\2\2\2\u0141\u0142\5w<\2\u0142"+
		"r\3\2\2\2\u0143\u0145\5k\66\2\u0144\u0143\3\2\2\2\u0145\u0146\3\2\2\2"+
		"\u0146\u0144\3\2\2\2\u0146\u0147\3\2\2\2\u0147t\3\2\2\2\u0148\u014a\7"+
		"\60\2\2\u0149\u014b\5k\66\2\u014a\u0149\3\2\2\2\u014b\u014c\3\2\2\2\u014c"+
		"\u014a\3\2\2\2\u014c\u014d\3\2\2\2\u014dv\3\2\2\2\u014e\u0150\t\t\2\2"+
		"\u014f\u0151\t\n\2\2\u0150\u014f\3\2\2\2\u0150\u0151\3\2\2\2\u0151\u0153"+
		"\3\2\2\2\u0152\u0154\5k\66\2\u0153\u0152\3\2\2\2\u0154\u0155\3\2\2\2\u0155"+
		"\u0153\3\2\2\2\u0155\u0156\3\2\2\2\u0156x\3\2\2\2\27\2\u00e3\u00ea\u00f0"+
		"\u00fe\u0107\u0109\u0110\u0112\u0116\u011e\u0123\u012a\u012e\u0135\u013b"+
		"\u013f\u0146\u014c\u0150\u0155\2";
	public static final ATN _ATN =
		new ATNDeserializer().deserialize(_serializedATN.toCharArray());
	static {
		_decisionToDFA = new DFA[_ATN.getNumberOfDecisions()];
		for (int i = 0; i < _ATN.getNumberOfDecisions(); i++) {
			_decisionToDFA[i] = new DFA(_ATN.getDecisionState(i), i);
		}
	}
}