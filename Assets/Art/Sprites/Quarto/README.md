# Sprites do Quarto

Pacote curado de 25 sprites PNG para a cena `Quarto` do Memento Mori.

## Estrutura

- `01_Tiles`: piso e segmentos de parede usados com Tilemap.
- `02_Arquitetura`: janela e entrada inferior.
- `03_Moveis`: móveis principais e auxiliares.
- `04_Decoracao_Props`: objetos decorativos e props separados.
- `05_Ritual`: tapete e itens do ritual.

## Inventário

### 01_Tiles

1. `01_tile_chao_madeira.png`
2. `02_segmento_parede_gotica.png`
3. `03_canto_parede_gotica.png`

### 02_Arquitetura

1. `01_janela_com_cortinas.png`
2. `02_entrada_porta_inferior.png`

### 03_Moveis

1. `01_cama.png`
2. `02_criado_mudo_com_vela.png`
3. `03_escrivaninha.png`
4. `04_banquinho.png`
5. `05_mesa_lateral_com_livro.png`
6. `06_altar_pequeno_com_vela.png`
7. `07_aparador_com_tigelas_e_planta.png`

### 04_Decoracao_Props

1. `01_retrato.png`
2. `02_boneco_de_pano.png`
3. `03_bau_pequeno.png`
4. `04_livro_fechado.png`
5. `05_livro_aberto.png`
6. `06_pena_e_tinteiro.png`
7. `07_pocao_frasco.png`

### 05_Ritual

1. `01_tapete_ritual_grande.png`
2. `02_base_quadrada_da_tigela.png`
3. `03_tigela_ritual_principal.png`
4. `04_vela_individual.png`
5. `05_bacia_ritualistica.png`
6. `06_tigela_com_cristais_roxos.png`

## Importação na Unity

1. Selecione os PNGs no Project window.
2. Defina `Texture Type` como `Sprite (2D and UI)`.
3. Use `Sprite Mode: Single` para props, móveis, arquitetura e ritual.
4. Para os arquivos de `01_Tiles`, mantenha um `Pixels Per Unit` consistente com o Tilemap do projeto; altere para `Multiple` e faça o slice somente se um arquivo for usado como tileset.
5. Use `Pixels Per Unit: 32`, `Filter Mode: Point (no filter)` e `Compression: None` para preservar pixel art.
6. Mantenha `Generate Mip Maps: Off` e o mesmo PPU nos sprites do quarto para conservar a escala relativa.

## Curadoria

Os arquivos têm nomes minúsculos, numerados e sem acentos. O pacote contém 25 PNGs com hashes distintos. Não há cortina separada: ela já está incorporada em `01_janela_com_cortinas.png`.

Rastreado na issue #16.
