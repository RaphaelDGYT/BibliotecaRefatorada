# Estudo de Caso: Refatoração de Sistema de Gerenciamento de Biblioteca - Parte 1

## Objetivo

Este documento descreve as violações dos princípios SOLID e das boas práticas de Clean Code encontradas no código original do sistema de gerenciamento de biblioteca, com o objetivo de fundamentar a refatoração implementada na Parte 2.

---

## 1. Violações Identificadas

### 🔴 1. Violação do SRP (Single Responsibility Principle)

- **Classe:** `GerenciadorBiblioteca`
- **Métodos:** `AdicionarLivro`, `AdicionarUsuario`, `RealizarEmprestimo`, `RealizarDevolucao`, `EnviarEmail`, `EnviarSMS`
- **Problema:** A classe centraliza múltiplas responsabilidades: cadastro de livros/usuários, lógica de empréstimos/devoluções, envio de notificações e controle de fluxo do sistema.
- **Justificativa:** O princípio SRP estabelece que uma classe deve ter apenas uma razão para mudar. Essa classe teria que ser modificada por diversos motivos diferentes (mudança na lógica de multas, envio de e-mails, cadastro de dados, etc.), o que dificulta a manutenção e aumenta o acoplamento.

---

### 🟠 2. Violação do OCP (Open/Closed Principle)

- **Classe:** `GerenciadorBiblioteca`
- **Método:** `RealizarEmprestimo`, `EnviarEmail`, `EnviarSMS`
- **Problema:** Para adicionar novas formas de notificação (ex: WhatsApp), é necessário modificar a classe existente.
- **Justificativa:** Segundo o OCP, uma classe deve estar aberta para extensão, mas fechada para modificação. A forma de envio de notificações deveria ser delegada a uma interface ou abstração, permitindo a adição de novas formas sem alterar o código existente.

---

### 🟡 3. Violação do DIP (Dependency Inversion Principle)

- **Classe:** `GerenciadorBiblioteca`
- **Método:** `EnviarEmail`, `EnviarSMS`
- **Problema:** A classe depende de implementações concretas para envio de notificações, e não de abstrações.
- **Justificativa:** O DIP prega que classes de alto nível não devem depender de classes de baixo nível, mas sim de interfaces. Isso prejudica a testabilidade e escalabilidade do sistema.

---

### 🔵 4. Violação do ISP (Interface Segregation Principle)

- **Classe:** `GerenciadorBiblioteca`
- **Problema:** Embora não existam interfaces explicitamente no código original, a classe acaba acumulando métodos de várias responsabilidades. Ao ser forçada a assumir várias tarefas, ela seria uma candidata a implementar múltiplas interfaces segregadas.
- **Justificativa:** O ISP recomenda que uma classe não seja forçada a depender de métodos que ela não usa. Essa violação ocorre implicitamente ao concentrar tudo em uma única classe.

---

### ⚫ 5. Violações de Clean Code (Boas práticas)

- **Classe:** `GerenciadorBiblioteca`
- **Problemas:**
  - Métodos longos com múltiplas responsabilidades.
  - Nomes de métodos e variáveis genéricos (ex: `l`, `u`, `livro`, `usuario`) que não expressam claramente suas intenções.
  - Repetição de lógica (ex: lógica de envio de notificações duplicada).
  - Retorno de `-1` como código de erro em `RealizarDevolucao`, o que é ambíguo.
- **Justificativa:** Clean Code prega clareza, nomes descritivos, funções coesas e curtas, uso de exceções em vez de códigos mágicos e eliminação de duplicação.

---

## 2. Consequências Técnicas das Violações

| Problema                         | Impacto Prático                              |
|----------------------------------|-----------------------------------------------|
| Alta coesão e acoplamento        | Dificuldade para testar e manter o sistema    |
| Baixa escalabilidade             | Problemas ao adicionar novos recursos         |
| Dificuldade de reutilização      | Lógica de notificações não é reaproveitável   |
| Manutenção arriscada             | Qualquer mudança pode gerar efeitos colaterais|
| Código pouco expressivo          | Dificulta o entendimento por novos devs       |

---

## 3. Conclusão

O código original possui múltiplas violações de princípios de design e práticas de código limpo. A classe `GerenciadorBiblioteca` é um anti-padrão de *God Object*, acumulando responsabilidades demais. A refatoração da Parte 2 visa corrigir essas deficiências aplicando SOLID e Clean Code, tornando o sistema mais modular, extensível e testável.

---
