# Memento Mori — Domínio da Lua — Unity Ready

Pacote final com 56 sprites individuais organizados para Unity.

## Importação recomendada
- Texture Type: Sprite (2D and UI)
- Sprite Mode: Single
- Pixels Per Unit: 32
- Filter Mode: Point (no filter)
- Compression: None
- Generate Mip Maps: Off

## Estrutura
- 01_Tiles_Base: IDs 01–12
- 02_Arquitetura: IDs 13–20
- 03_Puzzle_Espelhos: IDs 21–28
- 04_Puzzle_Sigilo: IDs 29–38
- 05_Props_Interativos: IDs 39–48
- 06_FX: IDs 49–56

## Observações técnicas
- Todos os arquivos são PNG RGBA.
- Os tamanhos foram normalizados para o footprint oficial.
- Redimensionamento de sprites existentes usa nearest-neighbor.
- Duplicatas e arquivos de referência/intermediários não entram no pacote.
- Variações necessárias (flip, espelho ativo/inativo, glow, segundo fragmento etc.) foram derivadas das artes existentes para manter consistência visual.
