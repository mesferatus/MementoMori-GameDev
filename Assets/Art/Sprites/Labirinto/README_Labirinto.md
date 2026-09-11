# Memento Mori — Labirinto (Unity Ready)

Pacote final com 71 sprites redimensionados tecnicamente para Unity, preservando a arte original.

## Importação na Unity

Para todos os PNGs:
- Texture Type: **Sprite (2D and UI)**
- Sprite Mode: **Single**
- Filter Mode: **Point (no filter)**
- Compression: **None**
- Pixels Per Unit: **32**
- Generate Mip Maps: **Off**

## Tamanhos finais

- 01–30 Tiles base: **32x32 px**
- 31–36 Arquitetura: **64x64 px**
- 37 Padlock: **32x32 px**
- 38–43 Banners: **64x128 px**
- 44–46 Props pequenos: **32x32 px**
- 47 Obelisk: **64x96 px**
- 48–49 Well/Altar: **64x64 px**
- 50–52 Posts: **32x64 px**
- 53–65 Circles/Icons/Sigils: **128x128 px**
- 66–71 Flames: **32x48 px**

## Tratamento aplicado

- PNG RGBA
- Transparência preservada
- Corte apenas da margem transparente externa
- Redimensionamento proporcional usando **Nearest Neighbor**
- Centralização em canvas transparente do tamanho final
- Nenhuma regeneração artística ou filtro de suavização
