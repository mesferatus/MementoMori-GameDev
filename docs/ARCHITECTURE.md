# Arquitetura

O jogo usa um fluxo de cenas do Unity: `MainMenu → Quarto → Labirinto → DominioLua → FinalBeta`.

- `Core`: estado da sessão, carregamento de cenas, bloqueios de entrada e configurações.
- `Player`: movimentação e câmera.
- `Interaction` e `Dialogue`: interação por proximidade, dados e apresentação de diálogos.
- `World`, `Puzzles` e `Poe`: gameplay de cada cena, flags de progresso e transições.
- `UI` e `Audio`: retorno visual e sonoro usado pelo jogo.
- `Tests`: testes EditMode e PlayMode dos sistemas principais.

O código mantém referências serializadas do Unity quando a configuração pertence à cena. Os sistemas globais são usados apenas em reações opcionais e locais.
