let _center = [141.35, 43.07];
let _zoom = 16;
let _pointGraphics = {};
let _mapGraphics = {}
let _markerGraphics = {}
let _textGraphics = {}
let _trajectoryInfo = {}
let arc_makePoly = null;
let arc_drawPoint = null;
let arc_removePoint = null;
let arc_createMap = null;
let arc_setColor = null;
let arc_drawMap = null;
let arc_removeMap = null;
let arc_createMarker = null;
let arc_drawMarker = null;
let arc_removeMarker = null;
let arc_drawTrajectory = null;
let arc_removeTrajectory = null;
let arc_setCenter = null;

export async function registerMap(id, map, color) {
  const paths = map.paths;
  const lines = [];
  for (let i = 0; i < paths.length; i++) {
    const points = paths[i].points;
    const lonlats = [];
    for (let j = 0; j < points.length; j++) {
      const point = points[j];
      const lat = point.latitude;
      const lon = point.longitude;
      lonlats.push([lon, lat]);
    }
    lines.push(arc_makePoly(lonlats));
  }
  arc_createMap(id, lines, color);
}

export async function setPathColor(id, index, color) {
  await arc_setColor(id, index, color);
}

export async function showMap(id) {
  await arc_drawMap(id);
}

export async function hideMap(id) {
  await arc_removeMap(id);
}

export async function drawPoint(id, lat, lon, color) {
  await arc_drawPoint(id, lat, lon, color);
}

export async function removePoint(id) {
  await arc_removePoint(id);
}

export async function drawMarker(id, lat, lon, rings, color) {
  _center = [lon, lat];
  const pt = [rings[0], rings[1]];
  const pl = [rings[2], rings[3]];
  const pb = [rings[4], rings[5]];
  const pr = [rings[6], rings[7]];
  rings = [[pt, pl, pb, pr, pt]];
  const symbol = arc_createMarker(rings, color);
  await arc_drawMarker(id, lat, lon, symbol);
}

export async function removeMarker(id) {
  await arc_removeMarker(id);
}

export async function addTrajectory(id, lat, lon) {
  await arc_drawTrajectory(id, lat, lon);
}
export async function removeTrajectory(id) {
  await arc_removeTrajectory(id);
}

export async function selectMarker(id) {
  const p = _markerGraphics[id].geometry;
  await arc_setCenter(p.longitude, p.latitude);
}

export async function removeAll(id) {
  await arc_removeMarker(id);
  await arc_removeTrajectory(id);
  await arc_removeMap(id);
  if (_markerGraphics[id])
    delete _markerGraphics[id];
  if (_mapGraphics[id])
    delete _mapGraphics[id];
  if (_trajectoryInfo[id])
    delete _trajectoryInfo[id];
}

require([
  "esri/Map",
  "esri/Graphic",
  "esri/layers/GraphicsLayer",
  "esri/views/MapView",
  "esri/widgets/BasemapToggle",
  "esri/geometry/Polyline",
  "esri/widgets/Search",
  "esri/geometry/coordinateFormatter",
  "esri/widgets/Fullscreen",
], (Map, Graphic, GraphicsLayer, MapView, BasemapToggle, Polyline, Search, coordinateFormatter, Fullscreen) => {

  coordinateFormatter.load();

  arc_makePoly = p => new Polyline({ paths: p });

  arc_drawPoint = async (id, lat, lon, color) => {
    const point = {
      type: "point",
      longitude: lon,
      latitude: lat
    };
    const simpleMarkerSymbol = {
      type: "simple-marker",
      color: color,
      size: 16,
      outline: {
        color: [ 0, 0, 0 ],
        width: 1
      }
    };
    const pointGraphic = new Graphic({
      layer: graphicsLayer,
      geometry: point,
      symbol: simpleMarkerSymbol
    });
    if (_pointGraphics[id])
      await view.graphics.remove(_pointGraphics[id]);
    _pointGraphics[id] = pointGraphic;
    await view.graphics.add(_pointGraphics[id]);
  }

  arc_removePoint = async id => {
    if (_pointGraphics[id])
      await view.graphics.remove(_pointGraphics[id]);
  }

  arc_createMap = (id, lines, color) => {
    const polylineSymbol = {
      type: "simple-line",
      color: color,
      width: 2,
    }
    const polyLines = [];
    for (var i = 0; i < lines.length; i++) {
      const polylineGraphic = new Graphic({
        layer: graphicsLayer,
        geometry: lines[i],
        symbol: polylineSymbol,
      });
      polyLines.push(polylineGraphic);
    }
    _mapGraphics[id] = polyLines;
  }

  arc_setColor = async (id, index, color) => {
    if (_mapGraphics[id][index]) {
      await view.graphics.remove(_mapGraphics[id][index]);
      _mapGraphics[id][index].symbol.color = color;
      await view.graphics.add(_mapGraphics[id][index]);
    }
  }

  arc_drawMap = async (id) => {
    if (_mapGraphics[id]) {
      const lines = _mapGraphics[id];
      for (var i = 0; i < lines.length; i++) {
        await view.graphics.remove(lines[i]);
        await view.graphics.add(lines[i]);
      }
    }
  }

  arc_removeMap = async id => {
    if (_mapGraphics[id]) {
      const lines = _mapGraphics[id];
      for (var i = 0; i < lines.length; i++) {
        await view.graphics.remove(lines[i]);
      }
    }
  }

  arc_createMarker = (rings, color) => {
    const sympoly = {
      type: "CIMPolygonSymbol",
      symbolLayers: [{
        type: "CIMSolidFill",
        width: 1,
        color: color,
      }]
    };
    const graphics = [{
      type: "CIMMarkerGraphic",
      geometry: {
        rings: rings
      },
      symbol: sympoly
    }];
    const layers = [{
      type: "CIMVectorMarker",
      enable: true,
      size: 40,
      frame: {
        xmin: -20,
        ymin: -20,
        xmax: 20,
        ymax: 20
      },
      markerGraphics: graphics
    }];
    const symbol = {
      type: "CIMPointSymbol",
      symbolLayers: layers
    };
    const marker = {
      type: "cim",
      data: {
        type: "CIMSymbolReference",
        symbol: symbol,
      },
    };
    return marker;
  }

  arc_drawMarker = async (id, lat, lon, symbol) => {
    const point = {
      type: "point",
      longitude: lon,
      latitude: lat
    };
    if (_markerGraphics[id])
      await view.graphics.remove(_markerGraphics[id]);
    _markerGraphics[id] = new Graphic({
      layer: graphicsLayer,
      geometry: point,
      symbol: symbol,
      attributes: {
        'name': id
      }
    });
    const textSymbol = {
      type: "text",
      color: "white",
      haloColor: "black",
      haloSize: "1px",
      text: id,
      xoffset: 0,
      yoffset: -45,
      font: {
        size: 18,
        weight: "bold"
      }
    };
    if (_textGraphics[id])
      await view.graphics.remove(_textGraphics[id]);
    _textGraphics[id] = new Graphic({
      layer: graphicsLayer,
      geometry: point,
      symbol: textSymbol,
      attributes: {
        'name': id
      }
    });
    await view.graphics.addMany([_markerGraphics[id], _textGraphics[id]]);
  }

  arc_removeMarker = async id => {
    if (_markerGraphics[id])
      await view.graphics.remove(_markerGraphics[id]);
    if (_textGraphics[id])
      await view.graphics.remove(_textGraphics[id]);
  }

  arc_drawTrajectory = async (id, lat, lon) => {
    const point = [lon, lat];
    if (_trajectoryInfo[id]) {
      const lp = _trajectoryInfo[id].points.slice(-1)[0];
      const dist = Math.pow(point[0] - lp[0], 2) + Math.pow(point[1] - lp[1], 2);
      if (dist > 2e-8) {
        _trajectoryInfo[id].points.push(point);
        const line = arc_makePoly(_trajectoryInfo[id].points);
        const polylineSymbol = {
          type: "simple-line",
          color: [200, 150, 0],
          width: 2,
        }
        const polylineGraphic = new Graphic({
          layer: graphicsLayer,
          geometry: line,
          symbol: polylineSymbol,
        });
        await view.graphics.remove(_trajectoryInfo[id].graphics);
        _trajectoryInfo[id].graphics = polylineGraphic;
        await view.graphics.add(_trajectoryInfo[id].graphics);
      }
    }
    else {
      _trajectoryInfo[id] = {
        points: [point],
        graphics: null
      };
    }

  }

  arc_removeTrajectory = async id => {
    if (_trajectoryInfo[id]) {
      await view.graphics.remove(_trajectoryInfo[id].graphics);
    }
  }

  arc_setCenter = (lon, lat) => {
    _center = [lon, lat];
    view.center = _center;
  }

  const graphicsLayer = new GraphicsLayer();

  const map = new Map({
    basemap: "satellite",
    layers: [graphicsLayer]
  });

  const view = new MapView({
    container: "viewDiv",
    map: map,
    zoom: _zoom,
    center: _center
  });

  const toggle = new BasemapToggle({
    view: view,
    nextBasemap: "topo-vector"
  });

  const searchWidget = new Search({ view: view });

  const fullscreen = new Fullscreen({ view: view });

  view.when(() => {
    view.ui.add(fullscreen, "bottom-right");
    view.ui.add(toggle, "bottom-right");
    view.ui.add(searchWidget, "top-right");
  });

  view.on("blur", function (e) {
    _center = view.center;
  });

  view.on("mouse-wheel", function (e) {
    _zoom = view.zoom;
  });

});
