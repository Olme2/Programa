-- ============================================================
-- Trigger nuevo: ajustar deuda cuando cambia "pagada" en compra
-- ============================================================
-- Por qué existe este trigger:
--   El repositorio, cuando solo cambian datos de cabecera (fecha,
--   proveedor, detalle, pagada) sin modificar los productos ni
--   cantidades, hace únicamente un UPDATE sobre la tabla compra
--   sin tocar detalle_compra. Por eso el trigger existente en
--   detalle_compra (tg_actualizar_deuda_proveedor) no se dispara.
--   Este trigger cubre ese caso.
--
-- Qué hace:
--   - pagada: FALSE → TRUE  →  resta la deuda total al proveedor
--   - pagada: TRUE  → FALSE →  suma la deuda total al proveedor
--
-- Qué NO hace:
--   - No se mete con stock (eso ya lo maneja tr_actualizar_stock_compra)
--   - No actúa en INSERT ni DELETE de compra
--   - No actúa si pagada no cambió
-- ============================================================

DROP TRIGGER IF EXISTS tg_compra_pagada_cambio ON public.compra;
DROP FUNCTION IF EXISTS fn_compra_pagada_cambio();

CREATE OR REPLACE FUNCTION fn_compra_pagada_cambio()
RETURNS TRIGGER AS $$
BEGIN
    -- Transición no pagada → pagada: restar deuda
    IF OLD.pagada = FALSE AND NEW.pagada = TRUE THEN
        UPDATE public.proveedor
        SET debo = debo - (
            SELECT COALESCE(SUM(dc.cantidad * dc.costo_unitario), 0)
            FROM public.detalle_compra dc
            WHERE dc.id_compra = NEW.id_compra
        )
        WHERE id_proveedor = NEW.id_proveedor;

    -- Transición pagada → no pagada: volver a sumar la deuda
    ELSIF OLD.pagada = TRUE AND NEW.pagada = FALSE THEN
        UPDATE public.proveedor
        SET debo = debo + (
            SELECT COALESCE(SUM(dc.cantidad * dc.costo_unitario), 0)
            FROM public.detalle_compra dc
            WHERE dc.id_compra = NEW.id_compra
        )
        WHERE id_proveedor = NEW.id_proveedor;
    END IF;

    RETURN NEW;
END;
$$ LANGUAGE plpgsql;

CREATE TRIGGER tg_compra_pagada_cambio
AFTER UPDATE OF pagada ON public.compra
FOR EACH ROW
EXECUTE FUNCTION fn_compra_pagada_cambio();
