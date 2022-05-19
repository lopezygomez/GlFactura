/* ************ GlFactura (PRINCIPAL) -------------------

    GlFactura.js    
*/

var PageSize = 28; // nº inicial de lineas por página --
var paramSelListaFacturas; // conserva parámetros de seleccion  
var ordenListaFacturas = 3;//  3->idFactura, 5->Fecha,  6->Cliente, 17->NombreCadena

var Hoy = new Date();
var fNueva = new Date(2018, 12 - 1, 15); // ref: Crea fecha (año,  mes(1 mas), dia),

var d = new Date();
var fechaActual = fechaddmm(d);

window.Global = { 
    infoServidor: {},
}

$(function () {
    //alert('$(function');
    //alert('Albaran inicial:'+$('#inicAlbaran').val()); // comprrueba si se ha recibido parámetro para abrir un alabran especifico --
    //alert('function: Inic jquery, Rolvalid:' + $('#Rolvalid').val());
    //alert("Rol:" + $("#Rol").val());

    $('[data-toggle="tooltip"]').tooltip();
    $("#lbVersion").html($("#infOculta").html().substring(0, 24));

    // asigna tooltip  de información, presenta Toda la informacíon que no entra en el boton --   
    $("#nvInfo").tooltip({
        title: $("#lbIp").html() + " " + $("#infOculta").html()
        , placement: 'bottom'
    });

    // Recupera informacion del la BD del servidor y demás --
    Global.infoServidor = recinfoServidor();
    //alert('infoServidor:' + JSON.stringify(Global.infoServidor));    
    $('#lbIp').attr('title', Global.infoServidor.BD);

    $.datepicker.setDefaults({
        dateFormat: 'dd/mm/yy',
        changeMonth: true, changeYear: true, numberOfMonths: 1,
        //showOn: "button", buttonImage: "img/Calendar_schedule.png",             
        showOn: "focus",
        dayNamesMin: ['Do', 'Lu', 'Ma', 'Mi', 'Ju', 'Vi', 'Sa'],
        monthNames: ['Enero', 'Febrero', 'Marzo', 'Abril', 'Mayo', 'Junio', 'Julio', 'Agosto', 'Septiembre', 'Octubre', 'Noviembre', 'Diciembre'],
        monthNamesShort: ['Ene', 'Feb', 'Mar', 'Abr', 'May', 'Jun', 'Jul', 'Ago', 'Sep', 'Oct', 'Nov', 'Dic']
    });

    var d = new Date();    //var strDate = d.getDate() + "/" + (d.getMonth()) + "/" + d.getFullYear();        
    $("#fHastaSel").val(d.getDate() + "/" + (d.getMonth() + 1) + "/" + d.getFullYear()); // fecha actual, (?? por algún motivo coje un mes menos )---
    $("#fFiltroHasta").val(d.getDate() + "/" + (d.getMonth() + 1) + "/" + d.getFullYear()); // fecha actual, (?? por algún motivo coje un mes menos )---

    // fecha desde,  5 dias menos --
    var dayOfMonth = d.getDate();
    d.setDate(dayOfMonth - 5); // resta días de la fecha actual --
    $("#fDesdeSel").val(d.getDate() + "/" + (d.getMonth() + 1) + "/" + d.getFullYear());
    $("#fFiltroDesde").val(d.getDate() + "/" + (d.getMonth() + 1) + "/" + d.getFullYear());

    $(".fecha").datepicker({});
    //selTodasFechas(); //activa o desactiva sel fechas, SINUSO   --

    if ($("#Rol").val() != 'ADMIN') {     $('#btNuevoAlb').hide();   };

    // Eventos -----------
    eventosClientes();

    // al  pulsar cualquier enlace en la barra de titulos, select al cambiar de elemento en barra navegacion principal, izquierda --
    //  $('#navTitulo select, #navTitulo a, #ulServicios, #ulRegular, #navMaestros li ').click(function (e) {
    $('#ulFacturas, #ulAlbaranes, #ulTrabajos, #navMaestros').click(function (e) {
        if (! cerrarEdicionesPendientes()) {
            //console.log('Cancelada Edicion de  Turnos');
            $('.capa').hide(); // oculta TODAS capas, incluso otros filtros  --   
            e.stopPropagation(); // cancela evento, 	        
        }
    });


    $body = $("body");
    $(document).ajaxStart(function () {
        //$('#msgAct').modal('show');// aqui , no se desactivaba siempre y bloqueaba la pagina al ser modal
        //console.log('ajaxStart :' + arguments.callee.name + minSecActual());
        $body.addClass("loading");                   
    });

    $(document).ajaxStop(function () {
        //console.log('ajaxStop :' + arguments.callee.name + minSecActual());        
        $body.removeClass("loading");
    });
    $(document).ajaxComplete(function () {
        //console.log('ajaxComplete :' + arguments.callee.name + minSecActual());
    });

    // asigna tooltips de  botones de maximizar, minimizar, proporcional y dinamico. OJO depende delos icionos para asignarlos	
    $(".BtnMaxMin .fa-list-alt").tooltip({ title: 'Maximiza Lista (Izquierda)' });
    $(".BtnMaxMin .fa-window-maximize").tooltip({ title: 'Maximiza Contenido (Derecha)' });
    $(".BtnMaxMin .fa-columns").tooltip({ title: 'Tamaño Proporcional' });
    $(".BtnMaxMin .fa-window-restore").tooltip({ title: 'Tamaño Dinamico' });

    // Eventos a controles varios -------

    // asigna evento para presentacion de capas --    $('.capa').hide(); // Cierra TODAS las capas --    $('#btPrueba').click(function (e) {
        pruebaWebServiceExterno();
    });
    $('#btFiltro').mouseover(function (e) {
        fncapacapaFiltroFacturas('#capaFiltroFacturas', e.pageX, e.pageY);
    });

    // asigna evento radio buttons de fechas predeterminadas --
    $('#rbSelF0, #rbSelF1, #rbSelF2, #rbSelF3, #rbSelF4 ').click(function (e) {
        selFechasPred();
    });


    // botones del cuerpo principal --        
    $('#btActualiza, #btOkFiltroServicios').click(function (e) {
        actListafacturas();
    });
    // al cambiar el nº de lineas por pagina o el ejercicio del filtro  --
    $('#selLinPag, #selEjercicio').change(function (e) {     actListafacturas();   });
    $('#btContabSel').click(function (e) {    contabSelecionadas();   });
    $('#btContabTodas').click(function (e) {     msgAler('Opción deshabilitada');   });

    $('#btListaFacturasExcel').click(function (e) {     ListaFacturasExcel();   });

    $('#btImprimeFacturasSel').click(function (e) {     imprimeSelecionadas();  });


    // botones  dentro del formulario de la factura "frmFactura" --
    $('#btGeneraSap').click(function (e) {      generarUnaFactura();   });
    $('#btImprimeFactura').click(function (e) {    btImprimeFactura();   });

    // aqui, sustituir por clase .numerico
    $('#txSelFactura, #txSelPedido, #txSelAlbaran, #txSelidSap, #frmEdClienteCodGraf, #frmEdClientePedMinimoConCompromiso, #frmEdClientePedMinimoSinCompromiso').numeric(false);
    $('#frmEdClienteLimiteCredito, #frmEdClienteDiasCredito').numeric(false);

    var d1 = new Date();
    $('#selEjercicio').val(d1.getFullYear()); // Filtro del ejercicio, por defecto el actual --

    crearPagerListaFacturas(5); //inicial con 5 paginas, aqui, ref: NO es el crearPager standar, Adaptar-- --
    selFechasPred(); // asigna antes el rango de fechas según el activo --
    actListafacturas();

    // recupera lista de clientes para usar en autocompletar, asigna evento a textbox  --    
    autocompletarClientes();

    jsCadenas = recListaCadenas(1); // recupera solo cadenas activas (1)
    cargaSelCadenas(jsCadenas);

    jsPaises = recListaPaises();
    cargaSelPaises(jsPaises);

    //jsTiposClientes = recListaTiposClientes();   //CargaSelTiposClientes(jsTiposClientes); // ahora se carga dinamicamente 

    jsTiposIva = recListaTiposIva();
    cargaSelTiposIva(jsTiposIva);

    jsFormasPago = recListaFormasPago();
    cargaSelFormasPago(jsFormasPago);
    

});// $(function --


function recinfoServidor() {
    $.ajax({
        type: 'POST', contentType: "application/json; charset=utf-8", dataType: "json", async: false,
        url: 'wsGlFactura.asmx/infoServidor',
        data: "{}",
        success: function (data) { js = JSON.parse(data.d); },
        error: function (request, status, error) { alert(request.responseText); }
    });
    return js;
}; // recinfoServidor --



// #region CargaSelec  ------------------

// Combos cargados DINAMICAMENTE --

function CargaSelTiposClientes(cadena) {
    lista = recListaTiposClientes(cadena);

    var selGeneral = "#frmEdClienteselTipoCliente";
    var selSinAsig = "#frmEdClienteselTipoCliente";
    $(selGeneral).html('');

    var options = '';
    $.each(lista, function (i, item) {
        options += "<option value='" + item.Codigo + "'>" + item.TipoClienteHotel + "</option>";
    });
    //$("#frmEdClienteselTipoCliente").append(options);
    $(selGeneral).append(options);
    if (lista == '')
        $(selGeneral).append('<option value="0" selected="selected">**SIN ASIGNAR**</option>');
    else
        $('<option value="0" selected="selected">**SIN ASIGNAR**</option>').insertBefore(selSinAsig + ' option:eq(0)');
}; // CargaSelTiposClientes --


function recListaTiposClientes(cadena) {
    $.ajax({    type: 'POST', contentType: "application/json; charset=utf-8", dataType: "json", async: false,
        url: 'wsGlFactura.asmx/recTiposClientes',
        data: '{Cadena:' + cadena + '}',
        success: function (data) { js = JSON.parse(data.d); },
        error: function (request, status, error) { alert(request.responseText); }
    });
    //console.log(JSON.stringify(js));
    return js;
};  // recListaTiposClientes


function CargaSelTipoPrecio(cadena, combo) {
    //alert('CargaSelTipoPrecio, cadena:' + cadena);
    $.ajax({
        type: 'POST', contentType: "application/json; charset=utf-8", dataType: "json", async: false,
        url: 'wsGlFactura.asmx/recTiposPrecios',
        data: '{Cadena:' + cadena + '}',
        success: function (data) { js = JSON.parse(data.d); },
        error: function (request, status, error) { alert(request.responseText); }
    });// $.ajax --     
    $(combo).html(''); // Vacia antes lista de items 

    // recorre lista recuperada y la asigna al combo 
    var options;
    $.each(js, function (i, item) {
        options += "<option value='" + item.Codigo + "'> " + item.TipoPrecio + "</option>";
    });
    $(combo).append(options);
    $('<option value="0" selected="selected">** Sin Asignar **</option>').insertBefore($(combo + ' option:eq(0)'));
};// CargaSelTipoPrecio --

function CargaSelMarca(cadena, combo) {
    //alert('CargaSelMarca, cadena:' + cadena);
    $.ajax({
        type: 'POST', contentType: "application/json; charset=utf-8", dataType: "json", async: false,
        url: 'wsGlFactura.asmx/recTiposMarcas',
        data: '{Cadena:' + cadena + '}',
        success: function (data) { js = JSON.parse(data.d); },
        error: function (request, status, error) { alert(request.responseText); }
    });// $.ajax --     
    $(combo).html(''); // Vacia antes lista de items 

    // recorre lista recuperada y la asigna al combo 
    var options;
    $.each(js, function (i, item) {
        options += "<option value='" + item.Codigo + "'> " + item.NombreMarca + "</option>";
    });
    $(combo).append(options);
    $('<option value="0" selected="selected">** Sin Asignar **</option>').insertBefore($(combo + ' option:eq(0)'));
};// CargaSelMarca --





function recListaPaises() {
    $.ajax({type: 'POST', contentType: "application/json; charset=utf-8", dataType: "json", async: false,
        url: 'wsGlFactura.asmx/recPaises',
        data: '{}',
        success: function (data) { js = JSON.parse(data.d); },
        error: function (request, status, error) { alert(request.responseText); }
    });
    return js;
};  // recListaPaises

function cargaSelPaises(Lista) {        
    var selGeneral = "#frmEdClienteSelPais";
    var selSinAsig = "#frmEdClienteSelPais";
    $(selGeneral).html('');
    var options = '';
    $.each(Lista, function (i, item) {
        options += "<option value='" + item.Codigo + "'>" + item.Pais + "</option>";
    });
    $(selGeneral).append(options);
    // Si no hay elementos en la lista --    
    if (Lista == '') 
        selGeneral.append('<option value="0" selected="selected">**SIN ASIGNAR**</option>');   
    else {                
        $('<option value="0" selected="selected">**SIN ASIGNAR**</option>').insertBefore(selSinAsig + ' option:eq(0)');
    };
}; // cargaSelPaises --

function recListaCadenas(activa) {
    //alert('recListaCadenas');       
    var paramSel = {        
         activo: activa // (1), 'false' (0), >1 Todos 	
        ,Cadena: 0 // (0) Todas 
    };
    $.ajax({ type: 'POST', contentType: "application/json; charset=utf-8", dataType: "json", async: false,
        url: 'wsGlFactura.asmx/recCadenas',        
        data: JSON.stringify(paramSel),
        success: function (data) { js = JSON.parse(data.d); },
        error: function (request, status, error) { alert(request.responseText); }
    });
    //console.log(JSON.stringify(js));
    return js;
};  // recListaCadenas

function cargaSelCadenas(Lista) {
    var selGeneral = ".selCadena";
    // Elementos a añadir **TODAS** al principio, usa tabla de objetos DOM (7/12/18) --
    var selTodas = [$("#selCadenaFiltro"), $("#selCadenaFiltroCapa")];    
    // Elementos a añadir **SIN ASIGNAR** al principio
    var selSinAsig = [$("#frmEdClienteselCadena")];
    
    $(selGeneral).html('');
    var options = '';
    $.each(Lista, function (i, item) {
        options += "<option value='" + item.Codigo + "'>" + item.NombreCadena + "</option>";
    });        
    $(selGeneral).append(options);
    
    //console.log('selCadenaFiltro primero:' + selTodas.find('option:first').html());
    if (Lista == '')      
        $(".selCadena").append('<option value="0" selected="selected">**SIN ASIGNAR**</option>');    
    else {                                
        $.each(selTodas, function (i, item) { $('<option value="0" selected="selected">**TODAS**</option>').insertBefore(item.find('option:first'));    });
        $.each(selSinAsig, function (i, item) { $('<option value="0" selected="selected">"**SIN ASIGNAR**</option>').insertBefore(item.find('option:first'));     });
    };    
    }
; // cargaSelCadenas --


function recListaTiposIva() {
    $.ajax({  type: 'POST', contentType: "application/json; charset=utf-8", dataType: "json", async: false,
        url: 'wsGlFactura.asmx/recTiposIva',
        data: '{}',
        success: function (data) { js = JSON.parse(data.d); },
        error: function (request, status, error) { alert(request.responseText); }
    });    
    return js;
};  // recListaTiposIva

function cargaSelTiposIva(Lista) {    
    var selGeneral = "#frmEdClienteselTipoIva";
    var selSinAsig = "#frmEdClienteselTipoIva";
    $(selGeneral).html('');
    var options = '';
    $.each(Lista, function (i, item) {
        options += "<option value='" + item.Codigo + "'>" + item.TipoIva + "</option>";
    });        
    $(selGeneral).append(options);    
    // Si no hay elementos en la lista --
    if (Lista == '') 
        selGeneral.append('<option value="0" selected="selected">**SIN ASIGNAR**</option>');    
    else        
        $('<option value="0" selected="selected">**SIN ASIGNAR**</option>').insertBefore(selSinAsig +' option:eq(0)');
}; // cargaSelTiposIva --


function recListaFormasPago() {
    $.ajax({  type: 'POST', contentType: "application/json; charset=utf-8", dataType: "json", async: false,
        url: 'wsGlFactura.asmx/recFormasPago',
        data: '{}',
        success: function (data) { js = JSON.parse(data.d); },
        error: function (request, status, error) { alert(request.responseText); }
    });
    //console.log(JSON.stringify(js));
    return js;
};  // recListaFormasPago


function cargaSelFormasPago(Lista) {
    var selGeneral = "#frmEdClienteSelFormaPago";
    var selSinAsig = "#frmEdClienteSelFormaPago";
    $(selGeneral).html('');
    var options = '';
    $.each(Lista, function (i, item) {
        options += "<option value='" + item.Codigo + "'>" + item.FormaPago + "</option>";
    });
    $(selGeneral).append(options);
    // Si no hay elementos en la lista --
    if (Lista == '')
        selGeneral.append('<option value="0" selected="selected">**SIN ASIGNAR**</option>');
    else
        $('<option value="0" selected="selected">**SIN ASIGNAR**</option>').insertBefore(selSinAsig + ' option:eq(0)');
}; // cargaSelFormasPago --




// Finciones de autocompletar --
function autocompletarClientes() {
    $.ajax({
        type: "POST", contentType: "application/json; charset=utf-8",
        url: "wsGlFactura.asmx/recClientesAutoC",
        async: true, data: "{}",
        dataType: "json",
        success: function (data) {
            var dataFromServer = data.d.split(":");
            $("#txCliente, #txSelCliente").autocomplete({
                source: dataFromServer,
                // asigna el valor del código a un segundo elemento al seleccionarlo --
                select: function (event, ui) {
                    Str = ui.item.value;
                    Codigo = parseInt(Str.substring(0, 4));
                    //alert(this.id); // ref: nombre del elemento llamador --ref: de otra manera --//alert(event.target.id);                                                 
                    if (this.id == 'txSelCliente')
                        $("#txSelIdCliente").val(Codigo);
                    if (this.id == 'txCliente')
                        $("#txIdCliente").val(Codigo);

                },
                change: function (event, ui) { // pone valor 0 si no se ha selecionado ninguno 
                    //alert('Change: NULO');     
                    if (ui.item == null) {
                        if (this.id == 'txSelCliente')
                            $("#txSelIdCliente").val(null);
                        if (this.id == 'txCliente')
                            $("#txIdCliente").val(null);
                    } else
                        // si el valor NO es cero y estamos cambiando cliente, asigna dirección al campo 
                        if (this.id == 'txCliente') {
                            //alert('txCliente asignado no nulo');
                            //anadirDireccion();
                        }
                }
                //,minLength: 4 //  nº de caracteres para comenzar búsqueda --
            });

        }, // function (data) -
        error:
            function (request, status, error) { alert(JSON.parse(request.responseText).Message); }
    }); // $.ajax({

}// autocompletarClientes --

// #endregion CargaSelec  ------------------



function creaTablaGeneral(tbl, js, columDef) {
    // genera tabla a partir de datos json usando una definición de columnas --
    //alert('creaTablaGeneral, tbl:' +tbl+', Datos:' + JSON.stringify(js));

    // Cabecera de la tabla. Recorre datos de definicion de columnas,  usa fila superior fija --    
    var htmCab = ' <thead class="cabecera-fija"> <tr >  ';
    $.each(columDef, function (i, item) {
        // si la longitud  es 0 la columna no es visible (20/8/18) 
        if (item.lon == 0)
            htmCab += '<th style="display:none;'
        else
            htmCab += '<th style="width:' + item.lon + 'px;';

        // alineacion de la columna --
        htmCab += 'text-align:' + item.alin + ';"'; //+ item.textoCab + ' </th> ';
        // si la columna es ordenable 
        if (item.ordenable)
            htmCab += ' class="cabOrdenable"';

        htmCab += '>' + item.textoCab + ' </th> ';

    });
    htmCab += '</tr> </thead> <tbody></tbody>';
    $(tbl).html(htmCab);

    // si no hay información finaliza --
    if ($.isEmptyObject(js))
        return;

    // Pasa ids de columna de 1a. fila de datos JSON  a array para realizar la busqueda a traves de funcion newArr --    
    var lisIdCol = [];
    var fila = js[0];// usa solo la primera fila de los datos para  busqueda de definiciones de colunmas --    
    $.each(Object.keys(fila), function (i, item) {
        //ref: usando indice del nombre objeto en lugar del item --
        //   lisIdCol.push(Object.keys(fila)[i]);
        lisIdCol.push(item);
    });

    // busca id de colummas a activar y crea lista con la posicion de cada una 
    var lisPosCol = [];
    $.each(columDef, function (i, item) {
        // ref: si existe en la tabla de ref de columnas,conserva su nº de columna, usa funcion inArray                
        var pos = $.inArray(item.idCol, lisIdCol);
        if (pos !== -1)
            lisPosCol.push(pos);
    });
    //alert(lisPosCol);    
    // Recorre datos JSON de detalles --    
    var htmLinDet = '';
    $.each(js, function (i, itemFila) {
        htmLinDet = '<tr>';
        // ref: usando indice de objeto en lugar del item 
        // var fila = js[i];        
        // recorre ids de columnas a presentar       
        $.each(lisPosCol, function (k, itemCol) {
            var col = lisPosCol[k];
            var conten = Object.values(itemFila)[itemCol];

            // si se trata de un checkbox  
            if (columDef[k].tipo == 'ckb') {
                var anulado = conten ? "checked" : "";
                //console.log('fila:' + i + ', Anulado:' + conten + ', Anulado:' + anulado);			    
                htmLinDet += '<td style="width:' + columDef[k].lon + 'px;text-align:center;" > ' +
                    '<input "' + Object.keys(itemFila)[itemCol] + '" type="checkbox" class="ckb" ' + anulado + ' disabled="disabled" /> </td>';
            }
            else {
                // si es una cadena, ajusta el texto a la Longitud, esta NO viene en caracteres, se divide 
                if (typeof conten == 'string')
                    conten = conten.substring(0, (columDef[k].lon) / 9);

                // si la longitud  es 0 la columna no es visible-- 
                if (columDef[k].lon == 0)
                    htmLinDet += '<td style="display:none;';
                else
                    htmLinDet += '<td style="width:' + columDef[k].lon + 'px; padding:2px;'

                htmLinDet += 'text-align:' + columDef[k].alin + ';"'
			     + ' id="' + Object.keys(itemFila)[itemCol] + '">'
			     + conten + '</td>';
            }
        });
        htmLinDet += '</tr>';
        $(tbl + ' tbody').append(htmLinDet);
    });
};// creaTablaGeneral --


function crearPager(Paginas, pager, fnCargaLista) {
    // aqui ref: pendiente, sustituir crearPagerListaFacturas por esta y eliminar despues--
    // ref: Recibe funcion fnCargaLista como parametro, solo pasa a su vez el nº de pagina resultante como paramatro (num)--
    // usa componente bootpag ref: http://botmonster.com/jquery-bootpag/#docs   

    $(pager).html(""); // elimina contenido del pager, 3/10/18 -- 

    $(pager).unbind('page'); //ELIMINAR antes el evento 'page' para que no se ejecute más veces --    
    $(pager).bootpag({
        total: Paginas,
        page: 1,   maxVisible: 5,    leaps: true,    firstLastUse: true,      first: '←', last: '( → ' + formatNum(Paginas.toFixed(0)) + ')'
        //wrapClass: 'pagination', activeClass: 'active', disabledClass: 'disabled', nextClass: 'next', prevClass: 'prev',lastClass: 'last',  firstClass: 'first',
    }).on("page", function (event, num) {
        //$(".content4").html("Page " + num); // or some ajax content loading...        
        //alert('EJECUTADO on page, ');                 
        //cargaPagServicios(num);
        fnCargaLista(num);
    });
    // Para compatibilizar bootpag compatible with Bootstrap 4.0 
    $(pager + ' li').addClass('page-item');
    $(pager + ' a').addClass('page-link');
}// crearPager --


function cerrarEdicionesPendientes() {
    //  comprueba cualquier panel de edición activo visible o formulario pendiente de modificacion., retorna false si abierto  --    
   
    if (formModificado('#frmEdCliente')) {
        //msgAler('Edición de Clientes pendiente');        
        if (! cierraEditCliente('#frmEdCliente'))
            return false;        
    };

   
    return true;
};// cerrarEdicionesPendientes --


function formModificado(div) {
    // retorna true si se ha modificado algun campo buscando la clase css de campo modificado 
    if ($(div + ' :input').hasClass('campoModif')) {
        return true;
    };
    return false;
    
} // formModificado -- 



// #region Factura  ------------------

function abrirFrmFactura(idFactura) {
    //alert('abrirFrmFactura');
    Frm = '#frmFactura';
    $('#txFactura').html(idFactura);
    $(Frm + ' :input').css('background-color', 'transparent'); // inicializa color de los campo de entrada, NO modificados -    
    $('#IdFacturaOculto').val(idFactura); // Conserva id de para acceso desde server--


    

    // Recupera datos de cabecera del factura  --   
    $.ajax({
        type: "POST", contentType: "application/json; charset=utf-8",
        async: false,
        url: "wsGlFactura.asmx/recDatFacturaJSON",
        data: '{idFactura: ' + idFactura +
              ', Ejercicio:' + $('#selEjercicio').val() +
            '}',
        dataType: "json",
        success:
            function (data) {
                var js = JSON.parse(data.d);
                $('#txFechaFactura').val(js.FacCabecera.FechaFactura);
                $('#txEjercicio').val(js.FacCabecera.Ejercicio);
                $('#txIdCliente').val(js.FacCabecera.idCliente);
                $('#txCliente').val(js.FacCabecera.NombreCliente);
                $('#txFContabilizada').val(js.FacCabecera.FContabilizada);
                $('#ckAbierta').prop('checked', false);
                if (js.FacCabecera.Estado == 0) {
                    $('#ckAbierta').prop('checked', true);
                }
                $('#txReferencia').val(js.FacCabecera.Referencia);
                $('#txCoste').val(js.FacCabecera.Coste);

                // si esta contabilizada ---                                
                if (js.FacCabecera.FContabilizada != '')
                    //$('#ocultofrmPeticion').hide();
                    $('#ocultofrmPeticion').show(); // aqui, anular,  con show  permite facturar YA facturadas --
                else
                    $('#ocultofrmPeticion').show()

                // ref. para comprobar valor nulo     //  if (data.d.Factura = ! null)                
                //$('#txIpMod').html(data.d.IpMod + ' (' + data.d.FechaMod + ')'); // la ip y la fecha de modif, en el mismo campo --   

                // Crea datatable con detalles recuperados en json --
                recDetallesFactura(js.DetFactura, '#gvDetallesFactura');
            },
        error:
            function (request, status, error) { alert(JSON.parse(request.responseText).Message); }
    }); // $.ajax({


    /*
    if ($("#Rol").val() != 'ADMIN') {
        if ($('#ocultofrmPeticion').is(":visible"))
            $('#ocultofrmPeticion, #ocultofrmPeticion2, #btsEdit').show()
        else
            $('#ocultofrmPeticion, #ocultofrmPeticion2, #btsEdit').hide();

        // deshabilita todos los controles de entrada  -- 
        $("#frmAlbaran input, #frmAlbaran textarea").attr('disabled', true);
        $('#btsDet').hide(); // botones de edición y alta de la linea de detalle --

    };

    // Campos permanentemente inhabilitados --
    $('#ckAnulado, #txIdCliente').attr('disabled', true);
    $('#pnEditLin').hide(); // panel de alta de linea oculto inicialmente --

    recDetallesAlbaran(idAlbaran);

    recPedido();

    // cambia colror de linea de tabla al pasar sobre ella --   
    //$("tr").not(':first').hover(  // ref: para todas las tablas --
    $("#gvDetallesAlbaran tr").not(':first').hover( // solo para gvDetallesAlbaran --
      function () {
          $(this).css("background", "grey");
      },
      function () {
          $(this).css("background", "");
      }
    );
    */
    $(Frm).modal('show');

}// abrirFrmFactura --

function recDetallesFactura(det, grid) {
    //alert('recDetallesFactura');
    $(grid + ' tbody').hide();
    $(grid + ' tbody').html('');
    var htmCab = ' <thead><tr>  ' +
         '<th style="display:none" /> <th/> <th/> <th/> <th/> <th/> <th/> <th/> <th/>  <th/>' +
         '</tr> </thead> <tbody></tbody>';
    $(grid).html(htmCab);
    // Recorre datos de detalles --
    for (var i = 0; i < det.length; ++i) {
        var htmLinDet = '<tr>' +
          '<td style="display:none">' + det[i].idFacturaDetalles + '</td>' +
          '<td style="width: 5%;">' + det[i].Fecha + '</td>' +
          '<td style="width: 40%;">' + det[i].Nombre + '</td>' +
          '<td style="width: 3%; text-align:right">' + det[i].Cantidad + '</td>' +
          '<td style="width: 4%; text-align:right">' + det[i].Precio.toFixed(2) + '</td>' +
          '<td style="width: 3%; text-align:right">' + det[i].PrecioUnitario.toFixed(5) + '</td>' +
          '<td style="width: 3%; text-align:right">' + det[i].ImporteIva.toFixed(2) + '</td>' +
          '<td style="width: 3%; text-align:right">' + det[i].Total.toFixed(2) + '</td>' +
          '<td style="width: 4%;">' + det[i].Presupuesto + '</td>' +
          '<td style="width: 4%;">' + det[i].Albaranes + '</td>' +
          '</tr>';
        $(grid + ' tbody').append(htmLinDet);
        // 24/11/20 Si la cantidad no esta indicada, genera mensaje SOLO de aviso.
        if (det[i].Cantidad == 0)
            alert('Cantidad en linea de albarán, NO Indicada ');
    };// for --

    
    $(grid).dataTable().fnDestroy();
    table = $(grid).dataTable({
        aoColumns: [{ sTitle: "idDet." }, { sTitle: "Fecha" }, { sTitle: "Nombre" }, { sTitle: "Cant." }
                  , { sTitle: "Precio" }, { sTitle: "P.Unid." }, { sTitle: "IVA" }, { sTitle: "Total" }, { sTitle: "Ppto." }, { sTitle: "Albaranes" }
        ],
        language: {
            lengthMenu: "_MENU_ lineas por página", paginate: { first: '« Primero', previous: '‹ Ant.', next: 'Sig. › ', last: 'Último »' },
            info: "_TOTAL_ Lineas (Pagina _PAGE_ de _PAGES_)", search: false,
            search: "Filtro sobre Resultado  ", infoFiltered: "(Filtradas  de _MAX_ total)", zeroRecords: "No existen lineas con este criterio de filtro",
        },
        bFilter: false, // oculta filtro --
        bLengthChange: false, // oculta lineas por página --                    
        paging: true, order: [[1, 'asc']], // order de columnas inicial,  -    
        lengthMenu: [10], select: true, select: { style: 'single' },
        // sDom: '<"top" i>rt<"bottom"flp><"clear">', // ref:el filtro y nº de lineas aparece abajo --
        //scrollY: 300  // ref: aqui, ojo.usando scrollY se desajustan las columnas --, Tamaño para scroll vertical, 
    });// dataTable --          
}//recDetallesFactura --

function btImprimeFactura() {
    // se ejecuta antes de btGeneraFacturaSrv_Click (runat server) ,
    var factura = $('#txFactura').html();
    var ejercicio = $('#txEjercicio').val();
    imprimeFactura(factura, ejercicio);
    // abre la factura en el explorador --      
    window.open('facturas/Fact_' + factura + '_' + ejercicio + '.pdf', '_blank');
}// btImprimeFactura --



function imprimeSelecionadas() {
    //alert('imprimeSelecionadas');
    // crea objeto json inicial para la lista de facturas --
    var lisFacturas = [{ idFactura: "0", Ejercicio: 0 }];
    lisFacturas.splice(0, 1); // Elimina el primer item (0) --

    // Recorre filas del grid SOLO seleccionadas --     
    $('#gvListaFacturas tr').filter(':has(:checkbox:checked)').each(function () {
        var fila = $(this);
        var filaInd = fila.parent().children().index($(this)); // indice de la fila --           
        var celSel = fila.find('#ckSellin'); // accede a columna especifica de fila por su id --                           
        // añade elemento al array JSON --                                
        lisFacturas.push({ idFactura: celSel.val(), Ejercicio: $('#selEjercicio').val() });

        imprimeFactura(celSel.val(), $('#selEjercicio').val());
        // abre la factura en el explorador --      
        window.open('facturas/Fact_' + celSel.val() + '_' + $('#selEjercicio').val() + '.pdf', '_blank');

    });
    //alert('imprimeSelecionadas: ' + JSON.stringify(lisFacturas));
    //mejora. Usar generarListaImprime (pendiente de desarrollo final) con una funcion en servidor para imprimir todas en un solo pdf, generarListaImprime.         
    //  generarListaImprime(lisFacturas);

};// imprimeSelecionadas  --


function generarListaImprime(lisFacturas) {
    // Imprime lista de facturas pasadas en JSON, en un solo pdf   ---
    // PENDIENTE DE DESAARROLLO,  Copiado de generarListaContab, finalizar desarrollo 28/3/18, usar ImprimeFactura de asmx --    
    //alert('generarListaImprime:' + JSON.stringify(lisFacturas));        
    /*
    $.ajax({
        type: "POST", async: true, contentType: "application/json; charset=utf-8",
        url: "wsGlFactura.asmx/generarContaFacturas",        
        data: JSON.stringify({      lisFacturas: lisFacturas    }),
        dataType: "json",
        success:
            function (data) {
                // cambia tamaño y texto de titulo de  pantalla del mensaje --
                $('#msgAlerTitulo').html("Resultado de Generación contable");
                $('#msgAler .modal-dialog').css("width", "380");
                $('#msgAler #txMsg').css("width", "320");
                $('#msgAler #txMsg').css("height", "300");
                msgAler(data.d);  // presenta aviso con el texto con el resultado  --
                // abre el fichero excel/csv en el explorador --
                var d = new Date();
                // calcula fecha actual añadiendo ceros al dia y mes --                
                fexcel = d.getFullYear() + ("00" + (d.getMonth() + 1)).slice(-2) + ("00" + d.getDate()).slice(-2)
                // aqui, proceso para pruebas, eliminar en produción, OJO: generar error si no existe alguno de los ficheros --
                window.open('Informes/HGAF' + fexcel + '.CSV', '_blank'); // abre excel en csv con contabilidad SAP de facturas --
                window.open('Informes/HGAC' + fexcel + '.CSV', '_blank'); // abre excel en csv con contabilidad SAP de costes  --

            },
        error: function (request, status, error) { alert(JSON.parse(request.responseText).Message); }
    }); // $.ajax({
    */

}// generarListaImprime  --



function imprimeFactura(factura, ejercicio) {
    $.ajax({     type: "POST", contentType: "application/json; charset=UTF-8", async: false,
        url: "wsGlFactura.asmx/ImprimeFactura",
        data: '{idFactura:' + factura +
            ', Ejercicio:' + ejercicio +
        '}',
        dataType: "json",
        success: function (data) { },
        error: {
            function (request, status, error) { alert(JSON.parse(request.responseText).Message); }
        },
    });
};// imprimeFactura --

// #endregion Factura  ------------------



// #region contable  ------------------

function contabSelecionadas() {
    // Crea lista de objetos de facturas en JSON para enviarlos como parametro --
    // ref: lisFacturas para pruebas con texto --
    // var lisFacturas = ' [' +  '{"idFactura": "1123", "Ejercicio": "2017"},' +     '{"idFactura": "1014", "Ejercicio": "2017"}' +    '] ';

    if (! confirm('¿ Contabilizar las Facturas ?')) {        
        return false;
    }
    
    $body.addClass("loading");
    // crea objeto json inicial para la lista de facturas --
    var lisFacturas = [{ idFactura: "0", Ejercicio: 0 }];
    lisFacturas.splice(0, 1); // Elimina el primer item (0) --

    // Recorre filas del grid SOLO seleccionadas --     
    $('#gvListaFacturas tr').filter(':has(:checkbox:checked)').each(function () {
        var fila = $(this);
        var filaInd = fila.parent().children().index($(this)); // indice de la fila --           
        var celSel = fila.find('#ckSellin'); // accede a columna especifica de fila por su id --                           
        // añade elemento al array JSON --                                
        lisFacturas.push({ idFactura: celSel.val(), Ejercicio: $('#selEjercicio').val() });
    });
    //alert(JSON.stringify(lisFacturas));    
    generarListaContab(lisFacturas);
    //console.log('FIn contabSelecionadas');
    
};// contabSelecionadas --


function generarUnaFactura() {
    // genera una solo factura, crea lista  JSON con UNA sola linea de factura  --
    var factura = $('#txFactura').html();
    var ejercicio = $('#txEjercicio').val();

    var lisFacturas = [{ idFactura: factura, "Ejercicio": ejercicio }];
    $body.addClass("loading");
    generarListaContab(lisFacturas);
    $body.removeClass("loading");
}; // generarUnaFactura --


function generarListaContab(lisFacturas) {
    //alert('lisFacturas:' + lisFacturas);
    // Genera contabilidad SAP de lista de facturas pasadas en  JSON  ---
    // Retorna texto con el resultado de cada una --    
    $.ajax({
        type: "POST", dataType: "json", contentType: "application/json; charset=utf-8", async: true,
        url: "wsGlFactura.asmx/generarContaFacturas",
        data: JSON.stringify({ lisFacturas: lisFacturas }),
        success:
            function (data) {
                // cambia tamaño y texto de titulo de  pantalla del mensaje --
                // 11/4/19, anulado, ahora saldra msg. fijo. $('#msgAlerTitulo').html("Resultado de Generación contable");
                $('#msgAler .modal-dialog').css("width", "380");
                $('#msgAler #txMsg').css("width", "320");
                $('#msgAler #txMsg').css("height", "300");
                msgAler(data.d);  // presenta aviso con el texto con el resultado  --
                // abre el fichero excel/csv en el explorador --
                var d = new Date();
                // calcula fecha actual añadiendo ceros al dia y mes --                
                fexcel = d.getFullYear() + ("00" + (d.getMonth() + 1)).slice(-2) + ("00" + d.getDate()).slice(-2)
                /* 30/7/18 anulado, proceso para pruebas, eliminar en produción, OJO: generar error si no existe alguno de los ficheros --
                window.open('Informes/HGAF' + fexcel + '.CSV', '_blank'); // abre excel en csv con contabilidad SAP de facturas --
                window.open('Informes/HGAC' + fexcel + '.CSV', '_blank'); // abre excel en csv con contabilidad SAP de costes  --
                */
                $body.removeClass("loading");
            },
        error: function (request, status, error) { alert(JSON.parse(request.responseText).Message); }
    }); // $.ajax({


    // Ref: probando retorno de objeto complejo datFactura (Contiene una cabecera y dos grupos de detalles ) --
    //$.ajax({
    //    type: "POST",   async: false,
    //    contentType: "application/json; charset=utf-8",
    //    url: "wsGlFactura.asmx/recDatFacturaJSON",
    //    data: '{idFactura:' + $('#txFactura').html() +    
    //       ', Ejercicio:' + $('#txEjercicio').val() +
    //     '}',
    //    dataType: "json",
    //    success:
    //        function (data) {                                                             
    //            var js = JSON.parse(data.d); 
    //            alert('data.d:' + data.d);                                
    //            //alert('js: Nº. factura:' + js.FacPrueba.idFactura + ', NombreCliente:' + js.FacPrueba.NombreCliente);
    //            //alert('js: Detalles:' + js.DetFactura[0].Nombre);                
    //        },
    //    error:    function (request, status, error) { alert(JSON.parse(request.responseText).Message); }
    //}); // $.ajax({
}// generarListaContab  --

// #endregion contable  ------------------




// #region otros --------------------



function msgAler(mensaje) {
    $('#msgAler').modal('show');
    //$('#txMsg').html(mensaje);
    $('#txMsg').val(mensaje);
}// msgAler --


function pruebaWebServiceExterno() {
    //alert('pruebaWebServiceExterno');

    $.ajax({
        type: "POST",
        contentType: "application/json; charset=utf-8",
        async: false,

        //url: "http://intranet.halconviajes.com/wsGAG/wsGAG_1.asmx/Hola",
        //data: '{ }',

        //url: "http://intranet.halconviajes.com/GlTrafico/wsGlTrafico.asmx",
        //data: '{ }',

        //-------------------------
        //url: "localhost:6070/www.dneonline.com/calculator.asmx?Add",
        //data: '{intA: 20' +
        //    ',intB: 25'+
        // '}', 

        //-------------------------    
        url: 'http://intranet.halconviajes.com/AeaStock/AeaStock.asmx/recStock',
        data: '{articulo: "157"' +
            ',ntrabajo: "2017110323"' +
            '}',

        dataType: "json",
        success:
            function (data) {
                //js = JSON.parse(data.d);
                alert(data);
            },
        error:
            function (request, status, error) {
                //alert('Error');
                //alert(JSON.parse(request.responseText).Message);
                alert(request.responseText);
            }
    }); // $.ajax({    


}//  pruebaWebServiceExterno --

function minSecActual() {
    // retorna minutos y segunos actuales para usarlo en debug 
    // PONERLA En util.js 
    return ' Min:' + (new Date()).getMinutes() + ', Seg:' + (new Date()).getSeconds();
}; // minSecActual


//  ************ SIN USO ********************
function selTodasFechas() {
    if ($('#ckTodasFechas').is(':checked'))
        $('#fDesdeSel, #fHastaSel').attr('disabled', true);
    else {
        $('#fDesdeSel, #fHastaSel').attr('disabled', false);
        //$('#fDesdeSel').focus(); 
    }
}// selTodasFechas --

// #endregion otros --------------------


// #region OBSOLETOS --------------------

function cargaCombosCadena_OBSOLETO() {
    // Obsoleta --
    $.ajax({
        type: "POST", contentType: "application/json; charset=utf-8", url: "wsGlFactura.asmx/recCadenasJSON", async: true,
        data: '{}',
        dataType: "json",
        success:
            function (data) {
                var js = JSON.parse(data.d);
                // Recorre detalles recuperados en json --
                for (var i = 0; i < js.length; ++i) {
                    $('#selCadenaFiltro').append('  <option value="' + js[i].Codigo + '" > ' + js[i].NombreCadena + ' </option>"');
                };
            },
        error:
            function (request, status, error) { alert(JSON.parse(request.responseText).Message); }
    }); // $.ajax({

}// cargaCombosCadena_OBSOLETO --

// #endregion OBSOLETOS 
