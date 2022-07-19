java -jar ./antlr-4.10.1-complete.jar ^
-Dlanguage=CSharp ^
-package "FlowChart.Parser.NodeParser" ^
-visitor ^
../../NewFlowChartTool/FlowChart.Parser/g4/NodeParser.g4 ^
-o ../../NewFlowChartTool/FlowChart.Parser/NodeParser