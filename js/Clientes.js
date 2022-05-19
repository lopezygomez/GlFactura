/* ************ Clientes -------------------
    Clientes.js    
*/


var GselListaClientes = {    
    Orden: 1, //   1-10-IdCliente,  2-20-NombreCadena, 3-idSap, 4-NIF, 5-TITULO(Razon Social), 6-60-POblacion, 7-70-Provincia
    Anulado: 3,  // 'true' (1), 'false' (0), >1 Todos 	--
    delGrupo: 3,
    idCliente: 0,
    idCadena: 0,
    idSap: '',
    CodExterno: '',
    NIF: '',
    RazonSocial: '',
    NombreComercial: '',
    Domicilio: '',
    Poblacion: ''    
};


function eventosClientes() {    
    $('#ulClientes, #btActClientes').click(function (e) { actListaClientes(); });
    $('#selLinPagClientes').change(function (e) { actListaClientes() });    
      
    /*
     al entrar o salir de Lista o edición de localizadores, cambia su tamaño         
     Tota suma de los 2 paneles 1300 px --

     Sutituido por BotonesMaxMin el 16/10/20

    $('#lisClientes').click(function (e) {
        $('#lisClientes').css('width', 1000);
        $('#frmEdCliente').css('width', 300);        

    });
    $('#frmEdCliente').click(function (e) {
        $('.capa').hide();
        $('#lisClientes').css('width', 400);
        $('#frmEdCliente').css('width', 900);
    });
    */

    BotonesMaxMin($('#lisClientes'), $('#frmEdCliente'), '#frmClientesBtnMaxMin', 1300, 80);

    // Si se modifica contenido de algún campo, cambia color del fondo  --       
    //  ojo en inputs con autocomplete , NO se cambia el color al modificarse --
    $("#frmEdCliente :input").unbind('change'); //ELIMINA antes el evento para que no se ejecute más veces cada vez que se cree --
    $("#frmEdCliente :input").change(function () {    
        $("#ocultofrmCliente").show();
        $(this).addClass('campoModif');
    });

    $('#btGuardarfrmCliente').click(function (e) { cierraEditCliente('#frmEdCliente'); });
    $('#btCancelafrmCliente').click(function (e) { cancelaEditCliente('#frmEdCliente'); });
    $('#frmEdClientebtNuevo').click(function (e) { nuevoCliente(); });

    $('#frmEdClientebtInformeExcel').click(function (e) {
        listaClientesExcel(GselListaClientes);
    });

    // Si cambia la cadena, cambia select/combos segun cadena de cliente   
    $('#frmEdClienteselCadena').on('change', function () {
        var cadena = $('#frmEdClienteselCadena').val();
        if (cadena !== '') {
            CargaSelTipoPrecio(cadena, '#frmEdClienteselTipoPrecio');
            CargaSelMarca(cadena, '#frmEdClienteselMarca');

            CargaSelTiposClientes(Lista);
        }
    });

};// eventosClientes --


// #region listaClientes ----------------

function actListaClientes() {
    //alert('actListaClientes');
    //GselListaClientes.Inactiva = 99; //99-todas, 0-Activas, 1-Inactivas

    var NReg = cuentaClientes(GselListaClientes);
    //alert('Total cuentaClientes:' + NReg);
    $('#lbnRegClientes').html(NReg + ' lin.');
    // ref: pasa funcion creaTablaLocaliza a crearPager como parametro, pasa a su vez en nº de pagina resultante como paramatro a recListaLocalizaPag(pagina)--    
    crearPager(Math.ceil(NReg / $('#selLinPagClientes').val()), '#pagerClientes', recClientesPag);    

    recClientesPag(1);

};// actListaClientes --

function recClientesPag(pagina) {
    // ref: OJO se llama tambien desde crearPager, solo admite el parametro "pagina" --        
    // si se estan recuperando registros, añade los parametros nPagina y pageSize y orden  --

     if (!cierraEditCliente())
        return;
    
   // GselListaClientes.nPagina = pagina;
   // GselListaClientes.pageSize = $('#selLinPagClientes').val();    
    
    recClientes(pagina, GselListaClientes);
};// recClientesPag --

function creaTablaClientes(js) {
    //console.log('creaTablaClientes, datos:' + JSON.stringify(js));    
    var columDef = [
      { idCol: "idCliente", lon: 60, textoCab: "Cod.", alin: "center", ordenable: true }
    , { idCol: "idSap", lon: 75, textoCab: "C.SAP",  ordenable: true}
    , { idCol: "CodExterno", lon: 80, textoCab: "C.Exter.", ordenable: true }
    , { idCol: "NIF", lon: 90, textoCab: "NIF", ordenable: true }
    , { idCol: "RazonSocial", lon: 230, textoCab: "Razon Social",  ordenable: true }
    , { idCol: "NombreComercial", lon: 200, textoCab: "Nombre Comercial",  ordenable: true}
    , { idCol: "NombreCadena", lon: 100, textoCab: "Cadena", ordenable: true }    
    , { idCol: "Anulado", lon: 60, textoCab: "Anul.", tipo: "ckb", alin: "center", ordenable: true }
    , { idCol: "delGrupo", lon: 60, textoCab: "Grupo", tipo: "ckb", alin: "center", ordenable: true }
    , { idCol: "Poblacion", lon: 120, textoCab: "Poblacion", ordenable: true }
    , { idCol: "Domicilio", lon: 120, textoCab: "Domicilio" }
    , { idCol: "Telef01", lon: 100, textoCab: "Telefono" }
    , { idCol: "Email", lon: 200, textoCab: "Correo" }    
    ];
    creaTablaGeneral('#tbClientes', js, columDef);    
    eventosTablaClientes('#tbClientes');
    
    // edita formulario del primer registro de la lista recuperado --
    //  busca 1er. localizador del grid --    
    var id = $('#tbClientes tbody tr').find('#idCliente').html();
    //console.log('id:' + id);
    if (id != undefined) {
        $('#frmEdCliente').show();
        abrirfrmEdCliente(id, '#frmEdCliente');
    }
    else {
        $('#frmEdCliente').hide();
    };
    
};// creaTablaClientes -

function eventosTablaClientes(tbl) {
    // añade eventos a la tabla, (solo body, no incluye thead ni tfoot)  --

    // cambia color del grid al pasar sobre el --     
    // aqui, error si hay ckb en la lista algunas veces nose desactiva la clase
    $(tbl + ' tbody tr').hover(function () {
        $(this).toggleClass('resaltaFila');        
    });
    
    // Click en fila, edita reg..    
    $(tbl + ' tbody tr ').click(function (e) {
        $('.capa').hide();
        if (cierraEditCliente('#frmEdCliente')) {
            $(tbl + ' tbody tr').removeClass('selFila'); // elimina antes clase de TODAS las filas sel body --
            $(this).addClass('selFila');
            var idCliente = $(this).find('#idCliente').html();            
            abrirfrmEdCliente(idCliente, '#frmEdCliente');
        };
    });    
    
    // recorre columnas de la cabecera para asignar evento de orden y cambiar formato de la cab     
    $(tbl + ' th').each(function () {
        // ref: otra forma de recorrer cabecera   //$('#tbTurnosTrayectos').find('thead th').each(function () {
        var celda = $(this);
        var titulo = celda.html().trim();
        // si esta ordenado por esa columna. añade clase con icono 
        //Orden: 1-10-idCliente,  2-20-NombreCadena, 3-idSap, 4-NIF, 5-TITULO(Razon Social), 6-60-POblacion, 7-70-Provincia, 8-TITULOL NombreComercial, 9-Borrado(Anulado)
        switch (titulo) {
            case 'Cod.': {
                if (GselListaClientes.Orden == 1)
                    celda.addClass('cabOrdenadaAsc')
                if (GselListaClientes.Orden == 10)
                    celda.addClass('cabOrdenadaDesc')
                if (GselListaClientes.idCliente != 0)
                    celda.addClass('cabFiltrada')
                break; }
            case 'C.SAP': {
                if (GselListaClientes.Orden == 3)
                    celda.addClass('cabOrdenadaAsc')             
                if (GselListaClientes.idSap != '')
                    celda.addClass('cabFiltrada')
                break;         }
            case 'C.Exter.': {
                if (GselListaClientes.Orden == 120)
                    celda.addClass('cabOrdenadaAsc')
                if (GselListaClientes.Orden == 121)
                    celda.addClass('cabOrdenadaDesc')
                if (GselListaClientes.CodExterno != '')
                    celda.addClass('cabFiltrada')
                break;        }
            case 'NIF': {
                if (GselListaClientes.Orden == 4)
                    celda.addClass('cabOrdenadaAsc')
                if (GselListaClientes.NIF != '')
                    celda.addClass('cabFiltrada')
                break;       }
            case 'Cadena': {
                if (GselListaClientes.Orden == 2)
                    celda.addClass('cabOrdenadaAsc')
                if (GselListaClientes.Orden == 20)
                    celda.addClass('cabOrdenadaDesc')
                if (GselListaClientes.idCadena != 0)
                    celda.addClass('cabFiltrada')                
                break;            }
            case 'Razon Social': {
                if (GselListaClientes.Orden == 5)
                    celda.addClass('cabOrdenadaAsc')
                if (GselListaClientes.RazonSocial != '')
                    celda.addClass('cabFiltrada')
                break;     }
            case 'Nombre Comercial': {
                if (GselListaClientes.Orden == 8)
                    celda.addClass('cabOrdenadaAsc')
                if (GselListaClientes.NombreComercial != '')
                    celda.addClass('cabFiltrada')
                break;           }
            case 'Anul.': {
                if (GselListaClientes.Orden == 9)
                    celda.addClass('cabOrdenadaAsc')
                if (GselListaClientes.Anulado != 3)
                    celda.addClass('cabFiltrada')
                break;          }
            case 'Grupo': {
                if (GselListaClientes.Orden == 11)
                    celda.addClass('cabOrdenadaAsc')
                if (GselListaClientes.delGrupo != 3)
                    celda.addClass('cabFiltrada')
                break;         }
            case 'Poblacion': {
                if (GselListaClientes.Orden == 6)
                    celda.addClass('cabOrdenadaAsc')
                if (GselListaClientes.Orden == 60)
                    celda.addClass('cabOrdenadaDesc')
                if (GselListaClientes.Poblacion != '')
                    celda.addClass('cabFiltrada')
                break;
            }
        };
        //Si la columna de columna contiene la clase "cabOrdenable", AÑADE AL FINAL evento para celda de cabecera de ordenación de columna
        if (celda.hasClass('cabOrdenable'))
            celda.click(function (e) { fnCapaOrdenCol('#CapaOrdenCol', celda); });
    });// each --

};// eventosTablaClientes --

function fnCapaOrdenCol(capa, celda) {
    // aqui copiado de gltrafico\localizadores.js..
    $('.capa').hide(); // oculta TODAS capas, incluso otros filtros  --   
    var titulo = celda.html().trim();
    // calcula desplazamiento de scroll horizontal del panel contenedor que emite el evento --
    var scLeft = $('#lisClientes').scrollLeft();
    $(capa).css('left', celda.position().left - scLeft + 95);
    $(capa).css('top', celda.position().top + 145);
    $(capa).show();

    // Oculta seleciones solo visibles según selección --    
    $('#dvFiltrosCapa DIV').hide();
    // botones de aceptar y cancelar filtro 
    $('#dvAceptarFiltro').show();

    //ELIMINA antes Eventos botones de busqueda y Ordenar     
    $('#btAceptarFiltro, #btCancelarFiltro, #btOrAsc, #btOrDesc').unbind('click');

    $(capa + ' .conDinamico').html('');
    $(capa).css('width', 250);
    $('#lbOrdenCol').html('Filtro/Orden ' + titulo);

    var txFiltro;
    switch (titulo) {
        case 'Cod.': {            
            $('#btOrAsc').click(function (e) { actOrdenCapa(1); });
            $('#btOrDesc').click(function (e) { actOrdenCapa(10); });
            /* Ref: creando contenido dinamico. TexBox con nº de localizador, debe de existir en html <div class="conDinamico">  </div>                    
                var htm = '<div class="form-group form-inline">' +
                    '  <input type="text" id="idClienteFiltroCapa" class="form-control" style="width:160px;" placeholder="Codigo "/> </div> ';
                $(capa + ' .conDinamico').html(htm);
            */                        
            txFiltro = $('#idClienteFiltroCapa');
            txFiltro.parent().show(); // presenta información SOLO de este filtro..
            // Recupera busqueda actual si hay alguno 
            txFiltro.val(GselListaClientes.idCliente);
            txFiltro.numeric(false);
            $('#btAceptarFiltro').click(function (e) { GselListaClientes.idCliente = txFiltro.val() == '' ? 0 : txFiltro.val(); });
            $('#btCancelarFiltro').click(function (e) {
                GselListaClientes.idCliente = 0;
                txFiltro.val('');
            });
            break;       }
        case 'C.SAP': {
            $('#btOrAsc, #btOrDesc').click(function (e) { actOrdenCapa(3); });
            txFiltro = $('#idSapFiltroCapa');
            txFiltro.parent().show(); 
            txFiltro.val(GselListaClientes.idSap);
            $('#btAceptarFiltro').click(function (e) { GselListaClientes.idSap = txFiltro.val(); });
            $('#btCancelarFiltro').click(function (e) {
                GselListaClientes.idSap = '';
                txFiltro.val('');
            });
            break;     }
        case 'C.Exter.': {
            $('#btOrAsc').click(function (e) { actOrdenCapa(120); });
            $('#btOrDesc').click(function (e) { actOrdenCapa(121); });            
            txFiltro = $('#CodExternoFiltroCapa');
            txFiltro.parent().show(); // presenta información SOLO de este filtro..
            // Recupera busqueda actual si hay alguno 
            txFiltro.val(GselListaClientes.CodExterno);            
            $('#btAceptarFiltro').click(function (e) { GselListaClientes.CodExterno = txFiltro.val() == '' ? 0 : txFiltro.val(); });
            $('#btCancelarFiltro').click(function (e) {
                GselListaClientes.CodExterno = '';
                txFiltro.val('');
            });
            break;    }
        case 'Cadena': {                        
            $('#btOrAsc').click(function (e) { actOrdenCapa(2); });
            $('#btOrDesc').click(function (e) { actOrdenCapa(20); });
            $('#dvAceptarFiltro').hide(); // oculta botones aceptar, cancelar filtro, el  filtro se activa al cambiar combo                                 
            txFiltro = $('#selCadenaFiltroCapa');
            txFiltro.parent().show();
            txFiltro.val(GselListaClientes.idCadena);
            // elimina eventos previamente para que no se acumulen   
            txFiltro.unbind('change'); 
            txFiltro.change(function (e) {
                GselListaClientes.idCadena = txFiltro.val();
                actListaClientes(); // es necesario actualizar en combos, no existen botones
            });
            break;       }
        case 'NIF': {
            $('#btOrAsc, #btOrDesc').click(function (e) { actOrdenCapa(4); });
            txFiltro = $('#NIFFiltroCapa');
            txFiltro.parent().show(); 
            txFiltro.val(GselListaClientes.NIF);
            $('#btAceptarFiltro').click(function (e) { GselListaClientes.NIF = txFiltro.val(); });
            $('#btCancelarFiltro').click(function (e) {
                GselListaClientes.NIF = '';
                txFiltro.val('');
            });
            break;       }
        case 'Razon Social': {
            $('#btOrAsc, #btOrDesc').click(function (e) { actOrdenCapa(5); });
            txFiltro = $('#RazonSocialFiltroCapa');
            txFiltro.parent().show(); 
            txFiltro.val(GselListaClientes.RazonSocial);
            $('#btAceptarFiltro').click(function (e) { GselListaClientes.RazonSocial = txFiltro.val(); });
            $('#btCancelarFiltro').click(function (e) {
                GselListaClientes.RazonSocial = '';
                txFiltro.val('');
            });
            break;        }
        case 'Nombre Comercial': {
            $('#btOrAsc, #btOrDesc').click(function (e) { actOrdenCapa(8); });
            txFiltro = $('#NombreComercialFiltroCapa');
            txFiltro.parent().show();
            txFiltro.val(GselListaClientes.NombreComercial);
            $('#btAceptarFiltro').click(function (e) { GselListaClientes.NombreComercial = txFiltro.val(); });
            $('#btCancelarFiltro').click(function (e) {
                GselListaClientes.NombreComercial = '';
                txFiltro.val('');
            });
            break;       }
        case 'Anul.': {
            $('#btOrAsc, #btOrDesc').click(function (e) { actOrdenCapa(9); });            
            $('#rbFiltroCapaAnulado').show();
            $('#rbFiltroCapaAnulado DIV').show();
            $('#btAceptarFiltro').click(function (e) {
                GselListaClientes.Anulado = $('input:radio[name=rbAnul]:checked').val();
                actListaClientes();                 
            });            
            $('#btCancelarFiltro').click(function (e) {
                GselListaClientes.Anulado = 3
                $('#rbTodosFiltroCapa').prop('checked', true);
                actListaClientes();                
            });
            break;      }
        case 'Grupo': {
            $('#btOrAsc, #btOrDesc').click(function (e) { actOrdenCapa(11); });
            $('#rbFiltroCapaDelGrupo').show();
            $('#rbFiltroCapaDelGrupo DIV').show();
            $('#btAceptarFiltro').click(function (e) {
                GselListaClientes.delGrupo = $('input:radio[name=rbGr]:checked').val();
                actListaClientes();
            });
            $('#btCancelarFiltro').click(function (e) {
                GselListaClientes.delGrupo = 3
                $('#rbTodosGrFiltroCapa').prop('checked', true);
                actListaClientes();
            });
            break;     }
        case 'Poblacion': {
            $('#btOrAsc').click(function (e) { actOrdenCapa(6); });
            $('#btOrDesc').click(function (e) { actOrdenCapa(60); });
            txFiltro = $('#PoblacionFiltroCapa');
            txFiltro.parent().show();
            txFiltro.val(GselListaClientes.Poblacion);
            $('#btAceptarFiltro').click(function (e) { GselListaClientes.Poblacion = txFiltro.val(); });
            $('#btCancelarFiltro').click(function (e) {
                GselListaClientes.Poblacion = '';
                txFiltro.val('');
            });
            break;      }
    };

    if (txFiltro != undefined)
        txFiltro.focus();

    // añade evento a botones ademas de los ya existentes 
    $('#btAceptarFiltro, #btCancelarFiltro').click(function (e) { actListaClientes(); });

}// fnCapaOrdenCol --

function actOrdenCapa(CampoOrden) {
    GselListaClientes.Orden = CampoOrden;
    actListaClientes();
}; // actOrdenCapa 

function cuentaClientes(paramSel) {
    //console.log('cuentaClientes, paramSel:' + JSON.stringify(paramSel));
    //  Recupera solo nº de registros y totales. 
    var NReg;
    $.ajax({   type: "POST", contentType: "application/json; charset=utf-8", dataType: "json", async: false,
        url: "wsGlFactura.asmx/cuentaClientes",
        //data: JSON.stringify(paramSel),
        data: '{Sel:' + JSON.stringify(paramSel) +'}',        
        success: function (data) { NReg = JSON.parse(data.d); },
        error: function (request, status, error) { alert(JSON.parse(request.responseText).Message); }
    }); // $.ajax --    
    return NReg; // retorna parametro JSON completo --

}// cuentaClientes --

function recClientes(pagina, paramSel) {    
    //console.log('recClientes, paramSel:' + JSON.stringify(paramSel));
    $.ajax({   type: 'POST', contentType: "application/json; charset=utf-8", dataType: "json", async: true,
        url: 'wsGlFactura.asmx/recClientes',        
        data: '{Sel:' + JSON.stringify(paramSel) + ',nPagina:' + pagina + ', pageSize:' + $('#selLinPagClientes').val() + '}',
        success: function (data) {
            js = JSON.parse(data.d);
            creaTablaClientes(js);
        },
        error: function (request, status, error) { alert(request.responseText); }
    });// $.ajax --     
        
} // recClientes --

function actListaUsuariosCliente(id) {
    js = recUsuariosCliente(id, 3); // 3-> todos    
    var columDef = [
      { idCol: "Empleado", lon: 90, textoCab: "Us./Empleado", alin: "center" }
    , { idCol: "NombreUsuario", lon: 200, textoCab: "Nombre Usuario" }
    , { idCol: "EMail", lon: 250, textoCab: "Correo" }
    , { idCol: "Activo", lon: 40, textoCab: "Activo", tipo: "ckb", alin: "center" }
    ];
    creaTablaGeneral('#tbUsuariosCliente', js, columDef);
    //eventosTablaUsuariosCliente('#tbUsuariosCliente');      
};// actListaUsuariosCliente--

function listaClientesExcel(paramSel) {
    //console.log('listaClientesExcel, paramSel:' + JSON.stringify(paramSel));
    $.ajax({  type: "POST", contentType: "application/json; charset=utf-8", dataType: "json", async: true,
        url: 'wsGlFactura.asmx/listaClientesExcel',          
        data: '{Sel:' + JSON.stringify(paramSel) + '}',
        success: function (data) {
            // abre el fichero en el explorador --
            window.open('Informes/listaClientes.xls', '_blank');
        },
    error: function (request, status, error) { alert(request.responseText); }
    });// $.ajax --  
    
    // abre el fichero en el explorador --
    //window.open('Informes/listaClientes.xls', '_blank');

}// listaClientesExcel--

function recUsuariosCliente(id, Activo) {
    // recupera usuarios de carrito de un cliente
    var selUsuariosCliente = {
        idCliente: id,
        Activo: Activo,  // 'true' (1), 'false' (0), >1 Todos 	--        
    };
    $.ajax({
        type: 'POST', contentType: "application/json; charset=utf-8", dataType: "json", async: false,
        url: 'wsGlFactura.asmx/recUsuariosCliente',
        data: JSON.stringify(selUsuariosCliente),
        success: function (data) { js = JSON.parse(data.d); },
        error: function (request, status, error) { alert(request.responseText); }
    });// $.ajax --  
    return js;
} // recListaServiciosLocaliza  --

// #endregion listaClientes 



// #region EditCliente ----------------

function abrirfrmEdCliente(id, frm) {
    //alert('abrirfrmEdCliente:' + id);    
    $('#frmEdClienteId').html(id);
    // inicializa color de los campo de entrada  y los  marca formulario como NO modificado aun --
    $(frm + ' :input').removeClass('campoModif');

    // recuperara info del cliente, 
    // usa de momento mismo aspx de la lista, fija obj JSon sin filtro, solo el id    
    var selListaClientes = {        
        Orden: 1,
        Anulado: 3,  // 'true' (1), 'false' (0), >1 Todos 	--
        delGrupo: 3, // 'true' (1), 'false' (0), >1 Todos 	--
        idCliente: id,
        idCadena: 0,
        idSap: '',
        CodExterno: '',
        NIF: '',
        RazonSocial: '',
        NombreComercial: '',
        Domicilio: '',
        Poblacion: ''
    };
    $.ajax({  type: "POST", contentType: "application/json; charset=utf-8", dataType: "json", async: false,
        url: 'wsGlFactura.asmx/recClientes',          
        data: '{Sel:' + JSON.stringify(selListaClientes) + ',nPagina:1, pageSize:1}',
        success:
            function (data) {
                var js = JSON.parse(data.d);
                //console.log('abrirfrmEdCliente,js:' + JSON.stringify(js));     
                $('#frmEdClienteTipoDoc').val(js[0].idTipoDocumento);
                $('#frmEdClienteNIF').val(js[0].NIF);
                $('#frmEdClienteidSap').val(js[0].idSap);
                $('#frmEdClienteCodExterno').val(js[0].CodExterno);
                $('#frmEdClienteCodGraf').val(js[0].CodGraf);
                $('#frmEdClienteCodContable').val(js[0].CodContable);
                $('#frmEdClienteRazonSocial').val(js[0].RazonSocial);
                $('#frmEdClienteNombreComercial').val(js[0].NombreComercial);
                $('#frmEdClienteDomicilio').val(js[0].Domicilio);
                $('#frmEdClientePoblacion').val(js[0].Poblacion);
                $('#frmEdClienteProvincia').val(js[0].Provincia);
                $('#frmEdClienteCodpostal').val(js[0].Codpostal);
                $('#frmEdClienteDomicilioEnvio').val(js[0].DomicilioEnvio);
                $('#frmEdClientePoblacionEnvio').val(js[0].PoblacionEnvio);
                $('#frmEdClienteProvinciaEnvio').val(js[0].ProvinciaEnvio);
                $('#frmEdClienteCodpostalEnvio').val(js[0].CodpostalEnvio);
                $('#frmEdClienteJefeEconomato').val(js[0].JefeEconomato);
                $('#frmEdClienteContrasena').val(js[0].Contrasena);
                $('#frmEdClienteSelPais').val(js[0].idPais);
                
                $('#frmEdClienteselTipoIva').val(js[0].idTipoIva);
                $('#frmEdClienteObservaciones').val(js[0].Observaciones);
                $('#frmEdClienteselCadena').val(js[0].idCadena);
                $('#frmEdClientePedMinimoConCompromiso').val(js[0].PedMinimoConCompromiso)
                $('#frmEdClientePedMinimoSinCompromiso').val(js[0].PedMinimoSinCompromiso)
                $('#frmEdClienteckAnulado').prop('checked', false);
                if (js[0].Anulado == 1)
                    $('#frmEdClienteckAnulado').prop('checked', true);
                $('#frmEdClienteckdelGrupo').prop('checked', false);
                if (js[0].delGrupo == 1)
                    $('#frmEdClienteckdelGrupo').prop('checked', true);
                $('#frmEdClienteckReqAutoriza').prop('checked', false);
                if (js[0].ReqAutoriza == true)
                    $('#frmEdClienteckReqAutoriza').prop('checked', true);
                $('#frmEdClienteSelFormaPago').val(js[0].idFormaPago);
                $('#frmEdClienteLimiteCredito').val(js[0].LimiteCredito);
                $('#frmEdClienteDiasCredito').val(js[0].DiasCredito);
                $('#frmEdClienteCuentaBancaria').val(js[0].CuentaBancaria);                
                $('#frmEdClienteTelef01').val(js[0].Telef01);
                $('#frmEdClienteEmail').val(js[0].Email);
                $('#frmEdClienteContacto').val(js[0].Contacto);                
                $('#frmEdClienteFAlta').val(js[0].FAlta);                

                let CadenaCombos = js[0].idCadena;
                // carga dinamicamente select/combo segun cadena de cliente                                         
                CargaSelTipoPrecio(CadenaCombos, '#frmEdClienteselTipoPrecio');
                $('#frmEdClienteselTipoPrecio').val(js[0].idTipoPrecio);
                CargaSelMarca(CadenaCombos, '#frmEdClienteselMarca');
                $('#frmEdClienteselMarca').val(js[0].idMarca);

                // Pendiente: 23/9/20 falta frmEdClienteselTipoCliente
                CargaSelTiposClientes(CadenaCombos);
                $('#frmEdClienteselTipoCliente').val(js[0].idTipoCliente);

            },
        error: function (request, status, error) { alert(JSON.parse(request.responseText).Message); }
    }); // $.ajax({

    // actualiza lista de usuarios de este cliente
    actListaUsuariosCliente(id); 

};// abrirfrmEdCliente --

function cierraEditCliente(frm) {
    // cierra la edición , valida y guarda previamente los datos, retorna false, si validación no correcta     
    // si NO se ha modificado algo, retorna sin hacer nada --
    
    
    if (!formModificado(frm))
        return true;
    
    // misma definicion que reg de reg de Estructuras.cs Cliente --       
    var regCliente = {
        idCliente: $('#frmEdClienteId').html() == 'Nuevo' ? 0 : $('#frmEdClienteId').html()
     , idSap: $('#frmEdClienteidSap').val()
     , DelGrupo: $('#frmEdClienteckdelGrupo').prop("checked") ? 1: 0
     , RazonSocial: $('#frmEdClienteRazonSocial').val()
     , NombreComercial: $('#frmEdClienteNombreComercial').val()
     , idTipoDocumento: $('#frmEdClienteTipoDoc').val()
     , NIF: $('#frmEdClienteNIF').val()
     , Domicilio: $('#frmEdClienteDomicilio').val()
     , Poblacion: $('#frmEdClientePoblacion').val()     
     , Provincia: $('#frmEdClienteProvincia').val()     
     , Codpostal: $('#frmEdClienteCodpostal').val()
     , DireccionEnvio: $('#frmEdClienteDomicilioEnvio').val()
     , PoblacionEnvio: $('#frmEdClientePoblacionEnvio').val()
     , ProvinciaEnvio: $('#frmEdClienteProvinciaEnvio').val()
     , CodpostalEnvio: $('#frmEdClienteCodpostalEnvio').val()
     , Telef01: $('#frmEdClienteTelef01').val()
     , Fax01: $('#frmEdClienteFax01').val()
     , Email: $('#frmEdClienteEmail').val()          
     , Observaciones: $('#frmEdClienteObservaciones').val() // text_Pago     
     , CodExterno: $('#frmEdClienteCodExterno').val() 
     , idCadena: $('#frmEdClienteselCadena').val()

       //, PedMinimoConCompromiso: $('#frmEdClientePedMinimoConCompromiso').val()
       //, PedMinimoSinCompromiso: $('#frmEdClientePedMinimoSinCompromiso').val()        
     , PedMinimoConCompromiso: !$('#frmEdClientePedMinimoConCompromiso').val() ? 0 : $('#frmEdClientePedMinimoConCompromiso').val()
     , PedMinimoSinCompromiso: !$('#frmEdClientePedMinimoSinCompromiso').val() ? 0 : $('#frmEdClientePedMinimoSinCompromiso').val()
     

     , Contrasena: $('#frmEdClienteContrasena').val()
     , FAlta: $('#frmEdClienteFAlta').val()
     , FBaja: $('#frmEdClienteFBaja').val()
     , Anulado: $('#frmEdClienteckAnulado').prop("checked")  ? true:false
     , ReqAutoriza: $('#frmEdClienteckReqAutoriza').prop("checked")
     , idTipoCliente: $('#frmEdClienteselTipoCliente').val()
     , JefeEconomato: $('#frmEdClienteJefeEconomato').val()
     , LimiteCredito: $('#frmEdClienteLimiteCredito').val()
     , DiasCredito:$('#frmEdClienteDiasCredito').val()
     , CuentaBancaria: $('#frmEdClienteCuentaBancaria').val()
     , idFormaPago: $('#frmEdClienteSelFormaPago').val()
     , FormaPago: ''
     , idMarca: !$('#frmEdClienteselMarca').val() ? 0 : $('#frmEdClienteselMarca').val()
     , idTipoPrecio: !$('#frmEdClienteselTipoPrecio').val() ? 0 : $('#frmEdClienteselTipoPrecio').val()
     , idTipo:0
     , idValidadora:0
     , Contacto: $('#frmEdClienteContacto').val()
     , idPais: $('#frmEdClienteSelPais').val()
     , idTipoIva: $('#frmEdClienteselTipoIva').val()
     , idTratoEspecial:0
     , CodContable: $('#frmEdClienteCodContable').val()
     , CodGraf: $('#frmEdClienteCodGraf').val()
    };

    /* ref: otra manera de enviar  parámetros JSON --
        var myJson = JSON.stringify(regCliente);// Necesita un stringify adicional ??? 
        -- en llamada $.ajax
        url: "wsGlFactura.asmx/pruebaJSONParam   ",
        data: '{orderJSON: '+ JSON.stringify(myJson) + '}',
    */
    //console.dir(regCliente);

    var resul = 0;
    if (validarCliente(regCliente)) {
        //console.log('CLIENTE GUARDADO '+JSON.stringify(regCliente));                
        $.ajax({  type: "POST", contentType: "application/json; charset=utf-8", dataType: "json", async: false,
            url: "wsGlFactura.asmx/actCliente   ", // retorna el nº de  creado o el modificado --      
            data: '{operacion:1 ,reg:' + JSON.stringify(regCliente) + '}',                        
            success: function (data) { resul = data.d; },
            error: function (request, status, error) { alert(JSON.parse(request.responseText).Message); }
        }); // $.ajax   
                        
        // si se trataba de una alta, asigna el nuevo id creado --
        if ($('#frmEdClienteId').html() == 'Nuevo') {
            //alert('Nuevo:' + resul);
            $('#frmEdClienteId').html(resul);
        }

        /* ref: refresca lista de la pagina actual, anul 11/12/18, se pierde la posición --
            // ref: busca pagina activa el  pager                  
            var pagActual = $('#pagerClientes  ul').find('.active .page-link').html();
            recClientesPag(pagActual);
        */
    }
    else
        return false;

    return true;
    
};//cierraEditCliente --



function cancelaEditCliente(frm) {
    // recupera de nuevo el registro actual --            
    if (formModificado(frm) && !confirm('Recuperar Informacion Original ?'))    
        //if (formModificado('#frmEdLocaliza') && !confirm('Recuperar Informacion Original ?'))
        return false;

    var idRec = $('#frmEdClienteId').html();
    // Si esta en alta, recuperará el primer registro de la lista, 1er localizador del grid --    
    if (idRec == 'Nuevo')
        idRec = $('#tbClientes tbody tr').find('#idCliente').html();
    
    abrirfrmEdCliente(idRec, '#frmEdCliente')

};// cancelaEditCliente --



function nuevoCliente() {
    //alert('Nuevo Cliente')
    Frm = '#frmEdCliente';
    if (! cierraEditCliente(Frm))
        return;

    // cambia  tamaño form edicion --        
    $("#frmEdCliente").trigger('click');

    // inicializa campos de entrada  
    $(Frm + ' :input').val('');    
    $('#frmEdClienteFAlta').val(fechaActual);
    $('#frmEdClienteFBaja').val('31/12/2100');    
    $(Frm + ' :checkbox').prop('checked', false);    

    $('#frmEdClienteselCadena').val(0); // **SIN ASIGNAR 
    $('#frmEdClienteSelPais').val(11); // 11-España
    $('#frmEdClienteselTipoIva').val(6); // IVA 21% --
    $('#frmEdClientePedMinimoConCompromiso').val(0);
    $('#frmEdClientePedMinimoSinCompromiso').val(0);
    $('#frmEdClienteSelFormaPago').val(1); // contado   
    $('#frmEdClienteLimiteCredito').val(0);
    $('#frmEdClienteDiasCredito').val(0);
    $('#frmEdClienteselTipoCliente').val(0);
    $('#frmEdClienteselTipoPrecio').val(0);
    $('#frmEdClienteselMarca').val(0);
    

    $('#frmEdClienteId').html('Nuevo');
    $('#frmEdClienteselCadena').focus();

    // Elimina lista de cliente, recupera cliente no existente    
    actListaUsuariosCliente(999999999);
}; // nuevoCliente -- 



function validarCliente(reg) {    
    if (reg.idCadena == 0 ) {
        msgAler('No se ha indicado Cadena');
        $('#frmEdClienteselCadena').focus();
        return false;
    };
    if (reg.RazonSocial == '') {
        msgAler('No se ha indicado Razon Social');
        $('#frmEdClienteRazonSocial').focus();
        return false;
    };
        
    if (!fechaValida(reg.FAlta)) {    
        msgAler('Fecha de Alta ' + reg.FAlta + ', Invalida ');
        $('#frmEdClienteFAlta').focus();
        return false;
    };
    // cambia fechas de texto a tipo date,  usa funcion textFecha, en util.js,  -- mal no detecta algunas fechas invalidas. solo para validar fecha y hora conjuntas--
    //var fs = textFecha(reg.FBaja);
    //if (fs == 'Invalid Date') {
    /*  Anulada temporalmente 
    if (!fechaValida(reg.FBaja)) {
        msgAler('Fecha de Baja' + reg.FBaja + ', Invalida ');
        $('#frmEdClienteFBaja').focus();
        return false;
    };
    */
    if (reg.NIF == '' ) {
        msgAler('No se ha indicado el NIF/DNI');
        $('#frmEdClienteNIF').focus();
        return false;
    };
    
    if (!soloLetrasyNum(reg.NIF)) {
        msgAler('El Documento Identificativo SOLO puede contener Letras y Números');
        $('#frmEdClienteNIF').focus();
        return false;
    };

    // Si el tipo de documento NO es tipo OTROS (3), valida 
    if (reg.idTipoDocumento != 3) {
        let resul = validarNIF(reg.NIF);
        if (resul == false) {
            msgAler('Documento NO VALIDO');
            $('#frmEdClienteNIF').focus();
            return false;
        }
        //console.log(reg.NIF + ':->' + resul);
        // si es CIF/NIF (1)    
        if ( reg.idTipoDocumento == 1 && (resul != 'DNI' &&  resul != 'CIF')) {
            msgAler('El Documento NO es un CIF, parece ser un ' + resul);
            $('#frmEdClienteNIF').focus();
            return false;            
        };
        // si es NIE (2 extranjero)    
        if (reg.idTipoDocumento == 2 && resul != 'NIE') {
            msgAler('El Documento NO es un NIE, parece ser un ' + resul);
            $('#frmEdClienteNIF').focus();
            return false;
        };
    };
   
    return true;
}// validarCliente --



// #endregion EditCliente 





