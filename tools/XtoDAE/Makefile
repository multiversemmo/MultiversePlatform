BUILD_CMD="/cygdrive/c/Windows/Microsoft.NET/Framework/v2.0.50727/MSBuild.exe"
ifndef CONFIG
CONFIG=Debug
# CONFIG=Release
endif

bin/${CONFIG}/xtodae.exe: parser.cs sym.cs xtodae.lex.cs
	${BUILD_CMD} /property:Configuration=$(CONFIG) xtodae.sln

parser.cs: xtodae.cup
	./C#Cup.exe $<

sym.cs: xtodae.cup
	./C#Cup.exe $<

xtodae.lex.cs: xtodae.lex
	./C#Lex.exe $<
