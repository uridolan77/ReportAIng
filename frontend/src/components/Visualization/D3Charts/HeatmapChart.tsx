import React, { useEffect, useRef, useState } from 'react';
import * as d3 from 'd3';
import { useComponentSize } from '../../../hooks/usePerformance';
import { useAccessibility } from '../../../hooks/useAccessibility';

interface HeatmapData {
  x: string;
  y: string;
  value: number;
  label?: string;
}

interface HeatmapChartProps {
  data: HeatmapData[];
  config: {
    title?: string;
    colorScheme?: string;
    showValues?: boolean;
    interactive?: boolean;
    margin?: { top: number; right: number; bottom: number; left: number };
  };
  onCellClick?: (data: HeatmapData) => void;
  onCellHover?: (data: HeatmapData | null) => void;
}

export const HeatmapChart: React.FC<HeatmapChartProps> = ({
  data,
  config = {},
  onCellClick,
  onCellHover
}) => {
  const svgRef = useRef<SVGSVGElement>(null);
  const tooltipRef = useRef<HTMLDivElement>(null);
  const { elementRef, size } = useComponentSize();
  const [selectedCell, setSelectedCell] = useState<HeatmapData | null>(null);
  const { announce } = useAccessibility();

  const {
    title = 'Heatmap',
    colorScheme = 'RdYlBu',
    showValues = true,
    interactive = true,
    margin = { top: 60, right: 60, bottom: 60, left: 60 }
  } = config;

  useEffect(() => {
    if (!svgRef.current || !data.length || size.width === 0) return;

    const svg = d3.select(svgRef.current);
    svg.selectAll('*').remove();

    const width = size.width;
    const height = Math.max(400, size.width * 0.6);
    const innerWidth = width - margin.left - margin.right;
    const innerHeight = height - margin.top - margin.bottom;

    // Set SVG dimensions
    svg.attr('width', width).attr('height', height);

    const g = svg.append('g')
      .attr('transform', `translate(${margin.left},${margin.top})`);

    // Get unique x and y values
    const xValues = Array.from(new Set(data.map(d => d.x))).sort();
    const yValues = Array.from(new Set(data.map(d => d.y))).sort();

    // Create scales
    const xScale = d3.scaleBand()
      .domain(xValues)
      .range([0, innerWidth])
      .padding(0.05);

    const yScale = d3.scaleBand()
      .domain(yValues)
      .range([0, innerHeight])
      .padding(0.05);

    // Color scale
    const colorScale = d3.scaleSequential(d3[`interpolate${colorScheme}` as keyof typeof d3] as any)
      .domain(d3.extent(data, d => d.value) as [number, number]);

    // Create tooltip
    const tooltip = d3.select(tooltipRef.current)
      .style('opacity', 0)
      .style('position', 'absolute')
      .style('background', 'rgba(0, 0, 0, 0.8)')
      .style('color', 'white')
      .style('padding', '8px 12px')
      .style('border-radius', '4px')
      .style('font-size', '12px')
      .style('pointer-events', 'none')
      .style('z-index', '1000');

    // Draw cells
    const cells = g.selectAll('.heatmap-cell')
      .data(data)
      .enter()
      .append('rect')
      .attr('class', 'heatmap-cell')
      .attr('x', d => xScale(d.x) || 0)
      .attr('y', d => yScale(d.y) || 0)
      .attr('width', xScale.bandwidth())
      .attr('height', yScale.bandwidth())
      .attr('fill', d => colorScale(d.value))
      .attr('stroke', '#fff')
      .attr('stroke-width', 1)
      .style('cursor', interactive ? 'pointer' : 'default');

    if (interactive) {
      cells
        .on('mouseover', function(event, d) {
          d3.select(this)
            .attr('stroke-width', 2)
            .attr('stroke', '#333');

          tooltip.transition()
            .duration(200)
            .style('opacity', 0.9);

          tooltip.html(`
            <strong>${d.label || `${d.x}, ${d.y}`}</strong><br/>
            Value: ${d.value.toFixed(2)}
          `)
            .style('left', (event.pageX + 10) + 'px')
            .style('top', (event.pageY - 28) + 'px');

          onCellHover?.(d);
        })
        .on('mouseout', function(event, d) {
          d3.select(this)
            .attr('stroke-width', 1)
            .attr('stroke', '#fff');

          tooltip.transition()
            .duration(500)
            .style('opacity', 0);

          onCellHover?.(null);
        })
        .on('click', function(event, d) {
          setSelectedCell(d);
          onCellClick?.(d);
          announce(`Selected cell: ${d.label || `${d.x}, ${d.y}`} with value ${d.value.toFixed(2)}`);
        });
    }

    // Add values to cells if enabled
    if (showValues) {
      g.selectAll('.heatmap-text')
        .data(data)
        .enter()
        .append('text')
        .attr('class', 'heatmap-text')
        .attr('x', d => (xScale(d.x) || 0) + xScale.bandwidth() / 2)
        .attr('y', d => (yScale(d.y) || 0) + yScale.bandwidth() / 2)
        .attr('text-anchor', 'middle')
        .attr('dominant-baseline', 'middle')
        .attr('fill', d => {
          const colorValue = colorScale(d.value);
          if (typeof colorValue === 'string') {
            const color = d3.color(colorValue);
            return color && d3.hsl(color).l > 0.5 ? '#000' : '#fff';
          }
          return '#000';
        })
        .attr('font-size', Math.min(xScale.bandwidth(), yScale.bandwidth()) / 4)
        .text(d => d.value.toFixed(1));
    }

    // Add axes
    const xAxis = d3.axisBottom(xScale);
    const yAxis = d3.axisLeft(yScale);

    g.append('g')
      .attr('class', 'x-axis')
      .attr('transform', `translate(0,${innerHeight})`)
      .call(xAxis)
      .selectAll('text')
      .style('text-anchor', 'end')
      .attr('dx', '-.8em')
      .attr('dy', '.15em')
      .attr('transform', 'rotate(-45)');

    g.append('g')
      .attr('class', 'y-axis')
      .call(yAxis);

    // Add title
    if (title) {
      svg.append('text')
        .attr('x', width / 2)
        .attr('y', 30)
        .attr('text-anchor', 'middle')
        .style('font-size', '16px')
        .style('font-weight', 'bold')
        .text(title);
    }

    // Add color legend
    const legendWidth = 200;
    const legendHeight = 10;
    const legendX = width - margin.right - legendWidth;
    const legendY = margin.top - 40;

    const legendScale = d3.scaleLinear()
      .domain(colorScale.domain())
      .range([0, legendWidth]);

    const legendAxis = d3.axisBottom(legendScale)
      .ticks(5)
      .tickFormat(d3.format('.1f'));

    const legend = svg.append('g')
      .attr('class', 'legend')
      .attr('transform', `translate(${legendX},${legendY})`);

    // Create gradient for legend
    const defs = svg.append('defs');
    const gradient = defs.append('linearGradient')
      .attr('id', 'heatmap-gradient')
      .attr('x1', '0%')
      .attr('x2', '100%')
      .attr('y1', '0%')
      .attr('y2', '0%');

    const numStops = 10;
    for (let i = 0; i <= numStops; i++) {
      const t = i / numStops;
      const value = colorScale.domain()[0] + t * (colorScale.domain()[1] - colorScale.domain()[0]);
      gradient.append('stop')
        .attr('offset', `${t * 100}%`)
        .attr('stop-color', colorScale(value));
    }

    legend.append('rect')
      .attr('width', legendWidth)
      .attr('height', legendHeight)
      .style('fill', 'url(#heatmap-gradient)');

    legend.append('g')
      .attr('transform', `translate(0,${legendHeight})`)
      .call(legendAxis);

  }, [data, size, config, onCellClick, onCellHover, announce, title, colorScheme, showValues, interactive, margin]);

  return (
    <div ref={elementRef} style={{ width: '100%', position: 'relative' }}>
      <svg
        ref={svgRef}
        role="img"
        aria-label={`${title} heatmap with ${data.length} data points`}
        style={{ width: '100%', height: 'auto' }}
      />
      <div ref={tooltipRef} />
      {selectedCell && (
        <div style={{ marginTop: '10px', padding: '8px', background: '#f5f5f5', borderRadius: '4px' }}>
          <strong>Selected:</strong> {selectedCell.label || `${selectedCell.x}, ${selectedCell.y}`} -
          Value: {selectedCell.value.toFixed(2)}
        </div>
      )}
    </div>
  );
};
