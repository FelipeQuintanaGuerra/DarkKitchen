"""
Gráfico A vs I (secuencia principal) — métricas de paquetes de DarkKitchen.
Uso:
    pip install matplotlib
    python grafico_secuencia_principal.py
Genera 'secuencia_principal.png' en la misma carpeta.
"""
import matplotlib.pyplot as plt

# (nombre, Instability, Abstractness)
paquetes = [
    ("Domain",         0.00, 0.04),
    ("DTOs",           0.33, 0.00),
    ("IBussinesLogic", 0.40, 1.00),
    ("IRepository",    0.40, 1.00),
    ("IImporters",     0.00, 0.50),
    ("Repository",     0.50, 0.00),
    ("BusinessLogic",  0.80, 0.00),
    ("ServiceFactory", 0.80, 0.00),
    ("WebApi",         1.00, 0.00),
]

fig, ax = plt.subplots(figsize=(8, 8))

# Zonas de peligro
ax.fill([0, 0.5, 0], [0, 0, 0.5], color="red", alpha=0.08)        # zona de dolor (abajo-izq)
ax.fill([1, 1, 0.5], [1, 0.5, 1], color="orange", alpha=0.08)     # zona de inutilidad (arriba-der)
ax.text(0.12, 0.06, "Zona de dolor", color="red", fontsize=9)
ax.text(0.62, 0.92, "Zona de inutilidad", color="darkorange", fontsize=9)

# Secuencia principal: A + I = 1
ax.plot([0, 1], [1, 0], "--", color="green", label="Secuencia principal (A + I = 1)")

# Puntos
for nombre, i, a in paquetes:
    ax.scatter(i, a, s=70, color="steelblue", zorder=3)
    ax.annotate(nombre, (i, a), textcoords="offset points",
                xytext=(7, 5), fontsize=9)

ax.set_xlim(-0.02, 1.05)
ax.set_ylim(-0.02, 1.05)
ax.set_xlabel("Inestabilidad (I)")
ax.set_ylabel("Abstracción (A)")
ax.set_title("Métricas de paquetes — Distancia a la secuencia principal\nDarkKitchen")
ax.grid(True, linestyle=":", alpha=0.5)
ax.legend(loc="upper center")

plt.tight_layout()
plt.savefig("secuencia_principal.png", dpi=150)
print("Generado: secuencia_principal.png")
