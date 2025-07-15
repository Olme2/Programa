--
-- PostgreSQL database dump
--

-- Dumped from database version 17.4
-- Dumped by pg_dump version 17.4

-- Started on 2025-07-11 00:17:49

SET statement_timeout = 0;
SET lock_timeout = 0;
SET idle_in_transaction_session_timeout = 0;
SET transaction_timeout = 0;
SET client_encoding = 'UTF8';
SET standard_conforming_strings = on;
SELECT pg_catalog.set_config('search_path', '', false);
SET check_function_bodies = false;
SET xmloption = content;
SET client_min_messages = warning;
SET row_security = off;

--
-- TOC entry 5026 (class 1262 OID 42574)
-- Name: polleria; Type: DATABASE; Schema: -; Owner: postgres
--

CREATE DATABASE polleria WITH TEMPLATE = template0 ENCODING = 'UTF8' LOCALE_PROVIDER = libc LOCALE = 'es-ES';


ALTER DATABASE polleria OWNER TO postgres;

\connect polleria

SET statement_timeout = 0;
SET lock_timeout = 0;
SET idle_in_transaction_session_timeout = 0;
SET transaction_timeout = 0;
SET client_encoding = 'UTF8';
SET standard_conforming_strings = on;
SELECT pg_catalog.set_config('search_path', '', false);
SET check_function_bodies = false;
SET xmloption = content;
SET client_min_messages = warning;
SET row_security = off;

--
-- TOC entry 4 (class 2615 OID 2200)
-- Name: public; Type: SCHEMA; Schema: -; Owner: pg_database_owner
--

CREATE SCHEMA public;


ALTER SCHEMA public OWNER TO pg_database_owner;

--
-- TOC entry 5027 (class 0 OID 0)
-- Dependencies: 4
-- Name: SCHEMA public; Type: COMMENT; Schema: -; Owner: pg_database_owner
--

COMMENT ON SCHEMA public IS 'standard public schema';


--
-- TOC entry 247 (class 1255 OID 42801)
-- Name: modificar_costo_promocionxdetallep(); Type: FUNCTION; Schema: public; Owner: postgres
--

CREATE FUNCTION public.modificar_costo_promocionxdetallep() RETURNS trigger
    LANGUAGE plpgsql
    AS 'DECLARE
	v_costo_prod producto.costo_producto%TYPE;
BEGIN
	CASE TG_OP
	WHEN ''INSERT'' THEN
		v_costo_prod = (SELECT costo_producto FROM producto WHERE id_producto = NEW.id_producto);
		UPDATE promocion SET costo_promocion = costo_promocion + v_costo_prod*NEW.cantidad, costo_venta = costo_venta + v_costo_prod*NEW.cantidad WHERE id_promocion = NEW.id_promocion;
		RETURN NEW;
	WHEN ''DELETE'' THEN
		v_costo_prod = (SELECT costo_producto FROM producto WHERE id_producto = OLD.id_producto);
		UPDATE promocion SET costo_promocion = costo_promocion - v_costo_prod*OLD.cantidad WHERE id_promocion = OLD.id_promocion;
		RETURN OLD;
	WHEN ''UPDATE'' THEN
		v_costo_prod = (SELECT costo_producto FROM producto WHERE id_producto = OLD.id_producto);
		UPDATE promocion SET costo_promocion = costo_promocion - v_costo_prod*OLD.cantidad + v_costo_prod*NEW.cantidad;
		RETURN NEW;
	END CASE;
END;';


ALTER FUNCTION public.modificar_costo_promocionxdetallep() OWNER TO postgres;

--
-- TOC entry 245 (class 1255 OID 42800)
-- Name: modificar_stock_productoxdetallec(); Type: FUNCTION; Schema: public; Owner: postgres
--

CREATE FUNCTION public.modificar_stock_productoxdetallec() RETURNS trigger
    LANGUAGE plpgsql
    AS 'BEGIN
	CASE TG_OP
	WHEN ''INSERT'' THEN
		UPDATE producto SET stock = stock + NEW.cantidad WHERE id_producto = NEW.id_producto;
		RETURN NEW;
	WHEN ''DELETE'' THEN
		UPDATE producto SET stock = stock - OLD.cantidad WHERE id_producto = OLD.id_producto;
		RETURN NEW;
	WHEN ''UPDATE'' THEN 
		UPDATE producto SET stock = stock - OLD.cantidad + NEW.cantidad WHERE id_producto = OLD.id_producto;
		RETURN NEW;
	END CASE;
END;';


ALTER FUNCTION public.modificar_stock_productoxdetallec() OWNER TO postgres;

--
-- TOC entry 244 (class 1255 OID 42799)
-- Name: modificar_stock_productoxdetallev(); Type: FUNCTION; Schema: public; Owner: postgres
--

CREATE FUNCTION public.modificar_stock_productoxdetallev() RETURNS trigger
    LANGUAGE plpgsql
    AS 'BEGIN
	CASE TG_OP
	WHEN ''INSERT'' THEN
		IF NEW.cantidad > (SELECT stock FROM producto WHERE id_producto = NEW.id_producto) THEN
			RAISE EXCEPTION ''No se puede seleccionar una cantidad que supere al stock del producto'';
		ELSE
			UPDATE producto SET stock = stock - NEW.cantidad WHERE id_producto = NEW.id_producto;
		END IF;
		RETURN NEW;
	WHEN ''DELETE'' THEN
		UPDATE producto SET stock = stock + OLD.cantidad WHERE id_producto = OLD.id_producto;
		RETURN NEW;
	WHEN ''UPDATE'' THEN 
		UPDATE producto SET stock = stock + OLD.cantidad - NEW.cantidad WHERE id_producto = OLD.id_producto;
		RETURN NEW;
	END CASE;
END;';


ALTER FUNCTION public.modificar_stock_productoxdetallev() OWNER TO postgres;

--
-- TOC entry 246 (class 1255 OID 42796)
-- Name: modificar_total_costo_ventaxdetalle(); Type: FUNCTION; Schema: public; Owner: postgres
--

CREATE FUNCTION public.modificar_total_costo_ventaxdetalle() RETURNS trigger
    LANGUAGE plpgsql
    AS 'DECLARE
	v_precio_prod producto.precio_producto%TYPE;
	v_costo_prod producto.costo_producto%TYPE;
BEGIN
	CASE TG_OP
	WHEN ''INSERT'' THEN
		v_precio_prod = (SELECT precio_producto FROM producto WHERE id_producto = NEW.id_producto);
		v_costo_prod = (SELECT costo_producto FROM producto WHERE id_producto = NEW.id_producto);
		IF NEW.promocion THEN
			UPDATE venta SET total = total + NEW.precio_promo, costo_venta = costo_venta + NEW.costo_promo WHERE id_venta = NEW.id_venta;
		ELSE
			UPDATE venta SET total = total + v_precio_prod*NEW.cantidad, costo_venta = costo_venta + v_costo_prod*NEW.cantidad WHERE id_venta = NEW.id_venta;
		END IF;
		RETURN NEW;
	WHEN ''DELETE'' THEN
		v_precio_prod = (SELECT precio_producto FROM producto WHERE id_producto = OLD.id_producto);
		v_costo_prod = (SELECT costo_producto FROM producto WHERE id_producto = OLD.id_producto);
		IF OLD.promocion THEN
			UPDATE venta SET total = total - OLD.precio_promo, costo_venta = costo_venta + OLD.costo_promo WHERE id_venta = OLD.id_venta;
		ELSE
			UPDATE venta SET total = total - v_precio_prod*OLD.cantidad, costo_venta = costo_venta - v_costo_prod*OLD.cantidad WHERE id_venta = OLD.id_venta;
		END IF;
		RETURN OLD;
	WHEN ''UPDATE'' THEN
		v_precio_prod = (SELECT precio_producto FROM producto WHERE id_producto = OLD.id_producto);
		v_costo_prod = (SELECT costo_producto FROM producto WHERE id_producto = OLD.id_producto);
		IF NEW.promocion THEN
			UPDATE venta SET total = total - OLD.precio_promo + NEW.precio_promo, costo_venta = costo_venta - OLD.costo_promo + NEW.costo_promo WHERE id_venta = OLD.id;
		ELSE
			UPDATE venta SET total = total - v_precio_prod*OLD.cantidad + v_precio_prod*NEW.cantidad, costo_venta = costo_venta - v_costo_prod*OLD.cantidad + v_costo_prod*NEW.cantidad;
		END IF;
		RETURN NEW;
	END CASE;
END;';


ALTER FUNCTION public.modificar_total_costo_ventaxdetalle() OWNER TO postgres;

SET default_tablespace = '';

SET default_table_access_method = heap;

--
-- TOC entry 222 (class 1259 OID 42659)
-- Name: compra; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public.compra (
    id_compra integer NOT NULL,
    total numeric(9,2) NOT NULL,
    fecha date NOT NULL,
    detalle character varying(100),
    CONSTRAINT precio_positivo CHECK ((total >= (0)::numeric))
);


ALTER TABLE public.compra OWNER TO postgres;

--
-- TOC entry 229 (class 1259 OID 42687)
-- Name: detalle_compra; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public.detalle_compra (
    id_compra integer NOT NULL,
    id_producto integer NOT NULL,
    id_proveedor integer NOT NULL,
    cantidad numeric(6,3) NOT NULL,
    CONSTRAINT cantidad_positiva CHECK ((cantidad >= (0)::numeric))
);


ALTER TABLE public.detalle_compra OWNER TO postgres;

--
-- TOC entry 219 (class 1259 OID 42610)
-- Name: detalle_promocion; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public.detalle_promocion (
    id_producto integer NOT NULL,
    id_promocion integer NOT NULL,
    cantidad numeric(6,3) NOT NULL,
    CONSTRAINT stock_positivo CHECK ((cantidad >= (0)::numeric))
);


ALTER TABLE public.detalle_promocion OWNER TO postgres;

--
-- TOC entry 221 (class 1259 OID 42642)
-- Name: detalle_venta; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public.detalle_venta (
    id_producto integer NOT NULL,
    id_venta bigint NOT NULL,
    cantidad numeric(6,3) NOT NULL,
    promocion boolean DEFAULT false NOT NULL,
    precio_promo numeric(7,2) DEFAULT 0 NOT NULL,
    costo_promo numeric(7,2) DEFAULT 0 NOT NULL,
    CONSTRAINT cantidad_positiva CHECK ((cantidad >= (0)::numeric))
);


ALTER TABLE public.detalle_venta OWNER TO postgres;

--
-- TOC entry 220 (class 1259 OID 42624)
-- Name: metodo_pago; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public.metodo_pago (
    id_metodo smallint NOT NULL,
    metodo character varying(30) NOT NULL
);


ALTER TABLE public.metodo_pago OWNER TO postgres;

--
-- TOC entry 231 (class 1259 OID 42731)
-- Name: producto; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public.producto (
    id_producto integer NOT NULL,
    id_proveedor integer NOT NULL,
    producto character varying(50) NOT NULL,
    stock numeric(6,3) NOT NULL,
    costo_producto numeric(7,2) NOT NULL,
    precio_producto numeric(7,2) NOT NULL,
    ganancia_producto numeric(7,2) GENERATED ALWAYS AS ((precio_producto - costo_producto)) STORED NOT NULL,
    porcentaje_ganancia_p numeric(5,2) GENERATED ALWAYS AS ((((precio_producto - costo_producto) / costo_producto) * (100)::numeric)) STORED NOT NULL,
    CONSTRAINT costo_positivo CHECK ((costo_producto >= (0)::numeric)),
    CONSTRAINT ganancia_positiva CHECK ((ganancia_producto >= (0)::numeric)),
    CONSTRAINT porcentaje_positivo CHECK ((porcentaje_ganancia_p >= (0)::numeric)),
    CONSTRAINT precio_mayor_que_costo CHECK ((precio_producto >= costo_producto)),
    CONSTRAINT precio_positivo CHECK ((precio_producto >= (0)::numeric)),
    CONSTRAINT stock_positivo CHECK ((stock >= (0)::numeric))
);


ALTER TABLE public.producto OWNER TO postgres;

--
-- TOC entry 218 (class 1259 OID 42586)
-- Name: productos; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public.productos (
    id_producto integer NOT NULL,
    id_proveedor integer NOT NULL,
    producto character varying(50) NOT NULL,
    stock numeric(6,3) NOT NULL,
    costo numeric(7,2) NOT NULL,
    precio_venta numeric(7,2) NOT NULL,
    ganancia numeric(7,2) NOT NULL,
    porcentaje_ganancia numeric(5,2) NOT NULL
);


ALTER TABLE public.productos OWNER TO postgres;

--
-- TOC entry 230 (class 1259 OID 42719)
-- Name: promocion; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public.promocion (
    id_promocion integer NOT NULL,
    promocion character varying(50) NOT NULL,
    costo_promocion numeric(7,2) NOT NULL,
    precio_promocion numeric(7,2) NOT NULL,
    ganancia_promocion numeric(7,2) GENERATED ALWAYS AS ((precio_promocion - costo_promocion)) STORED NOT NULL,
    porcentaje_ganancia_pp numeric(5,2) GENERATED ALWAYS AS ((((precio_promocion - costo_promocion) / costo_promocion) * (100)::numeric)) STORED NOT NULL,
    inicio date NOT NULL,
    fin date,
    CONSTRAINT costo_positivo CHECK ((costo_promocion >= (0)::numeric)),
    CONSTRAINT ganancia_positiva CHECK ((ganancia_promocion >= (0)::numeric)),
    CONSTRAINT inicio_antes_que_fin CHECK ((inicio <= fin)),
    CONSTRAINT porcentaje_positivo CHECK ((porcentaje_ganancia_pp >= (0)::numeric)),
    CONSTRAINT precio_positivo CHECK ((precio_promocion >= (0)::numeric))
);


ALTER TABLE public.promocion OWNER TO postgres;

--
-- TOC entry 217 (class 1259 OID 42581)
-- Name: proveedor; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public.proveedor (
    id_proveedor integer NOT NULL,
    proveedor character varying(50) NOT NULL,
    contacto character varying(30)
);


ALTER TABLE public.proveedor OWNER TO postgres;

--
-- TOC entry 227 (class 1259 OID 42680)
-- Name: secuencia_compras; Type: SEQUENCE; Schema: public; Owner: postgres
--

CREATE SEQUENCE public.secuencia_compras
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


ALTER SEQUENCE public.secuencia_compras OWNER TO postgres;

--
-- TOC entry 5028 (class 0 OID 0)
-- Dependencies: 227
-- Name: secuencia_compras; Type: SEQUENCE OWNED BY; Schema: public; Owner: postgres
--

ALTER SEQUENCE public.secuencia_compras OWNED BY public.compra.id_compra;


--
-- TOC entry 228 (class 1259 OID 42681)
-- Name: secuencia_metodos_de_pago; Type: SEQUENCE; Schema: public; Owner: postgres
--

CREATE SEQUENCE public.secuencia_metodos_de_pago
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


ALTER SEQUENCE public.secuencia_metodos_de_pago OWNER TO postgres;

--
-- TOC entry 223 (class 1259 OID 42676)
-- Name: secuencia_productos; Type: SEQUENCE; Schema: public; Owner: postgres
--

CREATE SEQUENCE public.secuencia_productos
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


ALTER SEQUENCE public.secuencia_productos OWNER TO postgres;

--
-- TOC entry 5029 (class 0 OID 0)
-- Dependencies: 223
-- Name: secuencia_productos; Type: SEQUENCE OWNED BY; Schema: public; Owner: postgres
--

ALTER SEQUENCE public.secuencia_productos OWNED BY public.producto.id_producto;


--
-- TOC entry 226 (class 1259 OID 42679)
-- Name: secuencia_promociones; Type: SEQUENCE; Schema: public; Owner: postgres
--

CREATE SEQUENCE public.secuencia_promociones
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


ALTER SEQUENCE public.secuencia_promociones OWNER TO postgres;

--
-- TOC entry 5030 (class 0 OID 0)
-- Dependencies: 226
-- Name: secuencia_promociones; Type: SEQUENCE OWNED BY; Schema: public; Owner: postgres
--

ALTER SEQUENCE public.secuencia_promociones OWNED BY public.promocion.id_promocion;


--
-- TOC entry 224 (class 1259 OID 42677)
-- Name: secuencia_proveedores; Type: SEQUENCE; Schema: public; Owner: postgres
--

CREATE SEQUENCE public.secuencia_proveedores
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


ALTER SEQUENCE public.secuencia_proveedores OWNER TO postgres;

--
-- TOC entry 5031 (class 0 OID 0)
-- Dependencies: 224
-- Name: secuencia_proveedores; Type: SEQUENCE OWNED BY; Schema: public; Owner: postgres
--

ALTER SEQUENCE public.secuencia_proveedores OWNED BY public.proveedor.id_proveedor;


--
-- TOC entry 232 (class 1259 OID 42760)
-- Name: venta; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public.venta (
    id_venta bigint NOT NULL,
    id_metodo smallint NOT NULL,
    total numeric(8,2) NOT NULL,
    costo_venta numeric(8,2) NOT NULL,
    ganancia_venta numeric(8,2) GENERATED ALWAYS AS ((total - costo_venta)) STORED NOT NULL,
    porcentaje_ganancia_v numeric(5,2) GENERATED ALWAYS AS ((((total - costo_venta) / costo_venta) * (100)::numeric)) STORED NOT NULL,
    fecha date NOT NULL,
    hora time without time zone NOT NULL,
    detalle character varying(100),
    CONSTRAINT costo_positivo CHECK ((costo_venta >= (0)::numeric)),
    CONSTRAINT ganancia_positiva CHECK ((ganancia_venta >= (0)::numeric)),
    CONSTRAINT porcentaje_positivo CHECK ((porcentaje_ganancia_v >= (0)::numeric)),
    CONSTRAINT total_mayor_que_costo CHECK ((total >= costo_venta)),
    CONSTRAINT total_positivo CHECK ((total >= (0)::numeric))
);


ALTER TABLE public.venta OWNER TO postgres;

--
-- TOC entry 225 (class 1259 OID 42678)
-- Name: secuencia_ventas; Type: SEQUENCE; Schema: public; Owner: postgres
--

CREATE SEQUENCE public.secuencia_ventas
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


ALTER SEQUENCE public.secuencia_ventas OWNER TO postgres;

--
-- TOC entry 5032 (class 0 OID 0)
-- Dependencies: 225
-- Name: secuencia_ventas; Type: SEQUENCE OWNED BY; Schema: public; Owner: postgres
--

ALTER SEQUENCE public.secuencia_ventas OWNED BY public.venta.id_venta;


--
-- TOC entry 5010 (class 0 OID 42659)
-- Dependencies: 222
-- Data for Name: compra; Type: TABLE DATA; Schema: public; Owner: postgres
--



--
-- TOC entry 5017 (class 0 OID 42687)
-- Dependencies: 229
-- Data for Name: detalle_compra; Type: TABLE DATA; Schema: public; Owner: postgres
--



--
-- TOC entry 5007 (class 0 OID 42610)
-- Dependencies: 219
-- Data for Name: detalle_promocion; Type: TABLE DATA; Schema: public; Owner: postgres
--



--
-- TOC entry 5009 (class 0 OID 42642)
-- Dependencies: 221
-- Data for Name: detalle_venta; Type: TABLE DATA; Schema: public; Owner: postgres
--



--
-- TOC entry 5008 (class 0 OID 42624)
-- Dependencies: 220
-- Data for Name: metodo_pago; Type: TABLE DATA; Schema: public; Owner: postgres
--



--
-- TOC entry 5019 (class 0 OID 42731)
-- Dependencies: 231
-- Data for Name: producto; Type: TABLE DATA; Schema: public; Owner: postgres
--



--
-- TOC entry 5006 (class 0 OID 42586)
-- Dependencies: 218
-- Data for Name: productos; Type: TABLE DATA; Schema: public; Owner: postgres
--



--
-- TOC entry 5018 (class 0 OID 42719)
-- Dependencies: 230
-- Data for Name: promocion; Type: TABLE DATA; Schema: public; Owner: postgres
--



--
-- TOC entry 5005 (class 0 OID 42581)
-- Dependencies: 217
-- Data for Name: proveedor; Type: TABLE DATA; Schema: public; Owner: postgres
--



--
-- TOC entry 5020 (class 0 OID 42760)
-- Dependencies: 232
-- Data for Name: venta; Type: TABLE DATA; Schema: public; Owner: postgres
--



--
-- TOC entry 5033 (class 0 OID 0)
-- Dependencies: 227
-- Name: secuencia_compras; Type: SEQUENCE SET; Schema: public; Owner: postgres
--

SELECT pg_catalog.setval('public.secuencia_compras', 1, false);


--
-- TOC entry 5034 (class 0 OID 0)
-- Dependencies: 228
-- Name: secuencia_metodos_de_pago; Type: SEQUENCE SET; Schema: public; Owner: postgres
--

SELECT pg_catalog.setval('public.secuencia_metodos_de_pago', 1, false);


--
-- TOC entry 5035 (class 0 OID 0)
-- Dependencies: 223
-- Name: secuencia_productos; Type: SEQUENCE SET; Schema: public; Owner: postgres
--

SELECT pg_catalog.setval('public.secuencia_productos', 1, false);


--
-- TOC entry 5036 (class 0 OID 0)
-- Dependencies: 226
-- Name: secuencia_promociones; Type: SEQUENCE SET; Schema: public; Owner: postgres
--

SELECT pg_catalog.setval('public.secuencia_promociones', 1, false);


--
-- TOC entry 5037 (class 0 OID 0)
-- Dependencies: 224
-- Name: secuencia_proveedores; Type: SEQUENCE SET; Schema: public; Owner: postgres
--

SELECT pg_catalog.setval('public.secuencia_proveedores', 1, false);


--
-- TOC entry 5038 (class 0 OID 0)
-- Dependencies: 225
-- Name: secuencia_ventas; Type: SEQUENCE SET; Schema: public; Owner: postgres
--

SELECT pg_catalog.setval('public.secuencia_ventas', 1, false);


--
-- TOC entry 4837 (class 2606 OID 42665)
-- Name: compra compra_pkey; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.compra
    ADD CONSTRAINT compra_pkey PRIMARY KEY (id_compra);


--
-- TOC entry 4796 (class 2606 OID 42605)
-- Name: productos costo_positivo; Type: CHECK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE public.productos
    ADD CONSTRAINT costo_positivo CHECK ((costo >= (0)::numeric)) NOT VALID;


--
-- TOC entry 4804 (class 2606 OID 42789)
-- Name: detalle_venta costo_positivo; Type: CHECK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE public.detalle_venta
    ADD CONSTRAINT costo_positivo CHECK ((costo_promo >= (0)::numeric)) NOT VALID;


--
-- TOC entry 4839 (class 2606 OID 42692)
-- Name: detalle_compra detalle_compra_pkey; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.detalle_compra
    ADD CONSTRAINT detalle_compra_pkey PRIMARY KEY (id_compra, id_producto, id_proveedor);


--
-- TOC entry 4831 (class 2606 OID 42684)
-- Name: detalle_promocion detalle_promocion_pkey; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.detalle_promocion
    ADD CONSTRAINT detalle_promocion_pkey PRIMARY KEY (id_producto, id_promocion);


--
-- TOC entry 4835 (class 2606 OID 42686)
-- Name: detalle_venta detalle_venta_pkey; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.detalle_venta
    ADD CONSTRAINT detalle_venta_pkey PRIMARY KEY (id_producto, id_venta);


--
-- TOC entry 4797 (class 2606 OID 42608)
-- Name: productos ganancia_positiva; Type: CHECK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE public.productos
    ADD CONSTRAINT ganancia_positiva CHECK ((ganancia >= (0)::numeric)) NOT VALID;


--
-- TOC entry 4833 (class 2606 OID 42628)
-- Name: metodo_pago metodo_pago_pkey; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.metodo_pago
    ADD CONSTRAINT metodo_pago_pkey PRIMARY KEY (id_metodo);


--
-- TOC entry 4798 (class 2606 OID 42609)
-- Name: productos porcentaje_positivo; Type: CHECK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE public.productos
    ADD CONSTRAINT porcentaje_positivo CHECK ((porcentaje_ganancia >= (0)::numeric)) NOT VALID;


--
-- TOC entry 4799 (class 2606 OID 42607)
-- Name: productos precioVenta_mayor_a_costo; Type: CHECK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE public.productos
    ADD CONSTRAINT "precioVenta_mayor_a_costo" CHECK ((precio_venta > costo)) NOT VALID;


--
-- TOC entry 4800 (class 2606 OID 42606)
-- Name: productos precioVenta_positivo; Type: CHECK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE public.productos
    ADD CONSTRAINT "precioVenta_positivo" CHECK ((precio_venta >= (0)::numeric)) NOT VALID;


--
-- TOC entry 4805 (class 2606 OID 42790)
-- Name: detalle_venta precio_mayor_a_costo; Type: CHECK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE public.detalle_venta
    ADD CONSTRAINT precio_mayor_a_costo CHECK ((precio_promo >= costo_promo)) NOT VALID;


--
-- TOC entry 4813 (class 2606 OID 42749)
-- Name: promocion precio_mayor_que_costo; Type: CHECK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE public.promocion
    ADD CONSTRAINT precio_mayor_que_costo CHECK ((precio_promocion >= costo_promocion)) NOT VALID;


--
-- TOC entry 4806 (class 2606 OID 42788)
-- Name: detalle_venta precio_positivo; Type: CHECK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE public.detalle_venta
    ADD CONSTRAINT precio_positivo CHECK ((precio_promo >= (0)::numeric)) NOT VALID;


--
-- TOC entry 4829 (class 2606 OID 42590)
-- Name: productos producto_pkey; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.productos
    ADD CONSTRAINT producto_pkey PRIMARY KEY (id_producto);


--
-- TOC entry 4843 (class 2606 OID 42743)
-- Name: producto producto_pkey1; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.producto
    ADD CONSTRAINT producto_pkey1 PRIMARY KEY (id_producto);


--
-- TOC entry 4841 (class 2606 OID 42730)
-- Name: promocion promocion_pkey1; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.promocion
    ADD CONSTRAINT promocion_pkey1 PRIMARY KEY (id_promocion);


--
-- TOC entry 4827 (class 2606 OID 42585)
-- Name: proveedor proveedor_pkey; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.proveedor
    ADD CONSTRAINT proveedor_pkey PRIMARY KEY (id_proveedor);


--
-- TOC entry 4801 (class 2606 OID 42604)
-- Name: productos stock_positivo; Type: CHECK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE public.productos
    ADD CONSTRAINT stock_positivo CHECK ((stock >= (0)::numeric)) NOT VALID;


--
-- TOC entry 4845 (class 2606 OID 42771)
-- Name: venta venta_pkey1; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.venta
    ADD CONSTRAINT venta_pkey1 PRIMARY KEY (id_venta);


--
-- TOC entry 4856 (class 2620 OID 42802)
-- Name: detalle_promocion modifica_costo; Type: TRIGGER; Schema: public; Owner: postgres
--

CREATE TRIGGER modifica_costo AFTER INSERT OR DELETE OR UPDATE OF cantidad ON public.detalle_promocion FOR EACH ROW EXECUTE FUNCTION public.modificar_costo_promocionxdetallep();


--
-- TOC entry 4859 (class 2620 OID 42805)
-- Name: detalle_compra modifica_stock; Type: TRIGGER; Schema: public; Owner: postgres
--

CREATE TRIGGER modifica_stock AFTER INSERT OR DELETE OR UPDATE OF cantidad ON public.detalle_compra FOR EACH ROW EXECUTE FUNCTION public.modificar_stock_productoxdetallec();


--
-- TOC entry 4857 (class 2620 OID 42804)
-- Name: detalle_venta modifica_stock; Type: TRIGGER; Schema: public; Owner: postgres
--

CREATE TRIGGER modifica_stock AFTER INSERT OR DELETE OR UPDATE OF cantidad ON public.detalle_venta FOR EACH ROW EXECUTE FUNCTION public.modificar_stock_productoxdetallev();


--
-- TOC entry 4858 (class 2620 OID 42806)
-- Name: detalle_venta modifica_total_costo; Type: TRIGGER; Schema: public; Owner: postgres
--

CREATE TRIGGER modifica_total_costo AFTER INSERT OR DELETE OR UPDATE OF cantidad, promocion, precio_promo, costo_promo ON public.detalle_venta FOR EACH ROW EXECUTE FUNCTION public.modificar_total_costo_ventaxdetalle();


--
-- TOC entry 4851 (class 2606 OID 42693)
-- Name: detalle_compra fk_compra_id_compra; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.detalle_compra
    ADD CONSTRAINT fk_compra_id_compra FOREIGN KEY (id_compra) REFERENCES public.compra(id_compra);


--
-- TOC entry 4855 (class 2606 OID 42772)
-- Name: venta fk_metodos_pago_id_metodo; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.venta
    ADD CONSTRAINT fk_metodos_pago_id_metodo FOREIGN KEY (id_metodo) REFERENCES public.metodo_pago(id_metodo);


--
-- TOC entry 4847 (class 2606 OID 42750)
-- Name: detalle_promocion fk_producto_id_producto; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.detalle_promocion
    ADD CONSTRAINT fk_producto_id_producto FOREIGN KEY (id_producto) REFERENCES public.producto(id_producto) ON UPDATE CASCADE ON DELETE CASCADE NOT VALID;


--
-- TOC entry 4849 (class 2606 OID 42778)
-- Name: detalle_venta fk_producto_id_producto; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.detalle_venta
    ADD CONSTRAINT fk_producto_id_producto FOREIGN KEY (id_producto) REFERENCES public.producto(id_producto) NOT VALID;


--
-- TOC entry 4852 (class 2606 OID 42791)
-- Name: detalle_compra fk_producto_id_producto; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.detalle_compra
    ADD CONSTRAINT fk_producto_id_producto FOREIGN KEY (id_producto) REFERENCES public.producto(id_producto) NOT VALID;


--
-- TOC entry 4848 (class 2606 OID 42755)
-- Name: detalle_promocion fk_promocion_id_promocion; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.detalle_promocion
    ADD CONSTRAINT fk_promocion_id_promocion FOREIGN KEY (id_promocion) REFERENCES public.promocion(id_promocion) ON UPDATE CASCADE ON DELETE CASCADE NOT VALID;


--
-- TOC entry 4846 (class 2606 OID 42591)
-- Name: productos fk_proveedor_id_proveedor; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.productos
    ADD CONSTRAINT fk_proveedor_id_proveedor FOREIGN KEY (id_proveedor) REFERENCES public.proveedor(id_proveedor);


--
-- TOC entry 4853 (class 2606 OID 42703)
-- Name: detalle_compra fk_proveedor_id_proveedor; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.detalle_compra
    ADD CONSTRAINT fk_proveedor_id_proveedor FOREIGN KEY (id_proveedor) REFERENCES public.proveedor(id_proveedor);


--
-- TOC entry 4854 (class 2606 OID 42744)
-- Name: producto fk_proveedor_id_proveedor; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.producto
    ADD CONSTRAINT fk_proveedor_id_proveedor FOREIGN KEY (id_proveedor) REFERENCES public.proveedor(id_proveedor);


--
-- TOC entry 4850 (class 2606 OID 42783)
-- Name: detalle_venta fk_venta_id_venta; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.detalle_venta
    ADD CONSTRAINT fk_venta_id_venta FOREIGN KEY (id_venta) REFERENCES public.venta(id_venta) ON UPDATE CASCADE ON DELETE CASCADE NOT VALID;


-- Completed on 2025-07-11 00:17:49

--
-- PostgreSQL database dump complete
--

