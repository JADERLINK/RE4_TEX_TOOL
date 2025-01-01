# RE4-TEX-TOOL
Extract and repack RE4 TEX files (RE4 UHD/PS4/NS/GC/WII/XBOX360)

**Translate from Portuguese Brazil**

Programa destinado a extrair e reempacotar arquivos .TEX;
<br> Ao extrair será gerado um arquivo de extenção .idxtex, ele será usado para o repack;
<br> Tool feita com referência a tool de Tex do Zatarita;
<br> Para saber para que serve esse arquivo, veja o arquivo tex do r315;

## Extract

Exemplo:
<br>RE4_TEX_TOOL_*.exe "r315_008.TEX"

! Vai gerar um arquivo de nome "r315_008.idxtex";
<br>! Vai criar arquivos TPL enumerados de nome: "r315_008_TEX_0.TPL";

## Repack

Exemplo:
<br>RE4_TEX_TOOL_*.exe "r315_008.idxtex"

! No arquivo .idxtex tem configs do arquivo tex;
<br>! Os arquivos Tpl têm que ter o mesmo nome que foram extraídos, enumerados em sequência;
<br>! No arquivo .idxtex as linhas iniciadas com um dos caracteres **# / \\ :** são consideradas comentários e não comandos validos;
<br>! O nome do arquivo gerado é o mesmo nome do idxtex, mas com a extenção .tex;

## Versions:
RE4_TEX_TOOL_GCWII.exe -> Re4 GC/WII
<br>RE4_TEX_TOOL_X360.exe -> Re4 X360
<br>RE4_TEX_TOOL_UHD.exe -> Re4 UHD
<br>RE4_TEX_TOOL_PS4NS.exe -> Re4 PS4/NS

**At.te: JADERLINK**
<br>Thanks to "Zatarita"
<br>2025-01-01