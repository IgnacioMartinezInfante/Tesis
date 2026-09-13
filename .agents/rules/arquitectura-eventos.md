---
trigger: always_on
---

El sistema que controla la historia no debe instanciar ni reproducir audio directamente. Solo debe emitir eventos (ej: OnNodeReached). El AudioManager debe suscribirse y reaccionar a estos eventos para mantener el desacople de cara a la integración en segundo plano.