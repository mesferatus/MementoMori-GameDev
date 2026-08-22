# C4C — Classificação do Domínio da Lua

## Implementar agora

`D01`, `D04`, `D05`, `D06`, `D07`, `D08`, `D09`, `D10`, `D11`, `D12`, `D13`, `D15`, `D16`, `D17`, `D18`, `D20`, `D21`, `D22`.

Esses IDs usam `DialogueData`, estados existentes, contadores de erro, reações de Poe e o evento de conclusão dos puzzles. Não exigem posição final, prop final, animação final ou UI final.

## Visual-dependent pending

`D02`, `D03`, `D14`, `D19`.

`D02` depende da leitura de uma área final do domínio; `D03` e `D19` dependem da interação com props/elementos visuais específicos; `D14` depende do hook da animação final dos espelhos. Permanecem reservados para C6.

## Optional deferred

Nenhuma entrada obrigatória independente de visual foi descartada. `D02` permanece opcional e visual-dependent; não foi substituída por placeholder.

## Preservação de regras

- Jardim mantém a regra Crescente → Cheia → Minguante e a ordem real das flores minguantes.
- Espelhos continuam aceitando apenas os símbolos publicados, sem resetar progresso confirmado.
- Corredor continua recuperável e ambíguo; o feedback não revela a rota.
- Sigilo continua Lua → Olho → Espiral, com inputs duplicados bloqueados pela conclusão única.
- Fragmento continua liberado apenas após `SigilPuzzleComplete` e define `FragmentCollected` no fluxo correto.
