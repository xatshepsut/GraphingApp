# https://networkx.github.io/documentation/latest/reference/generators.html

import click
import networkx as nx
from enum import Enum


class GraphType(Enum):
	Path = 'path'
	Cycle = 'cycle'
	Star = 'star'
	Complete = 'complete'
	Hypercube = 'hypercube'
	Wheel = 'wheel'
	Random = 'random'
	
	@classmethod
	def fromstring(cls, string):
		for key, value in vars(cls).iteritems():
			if key == string.lower().title():
				return value


def generate_graph(type, n):
	graph = nx.empty_graph()
	
	if GraphType.Path == type:
		graph = nx.path_graph(n)
	elif GraphType.Cycle == type:
		graph = nx.cycle_graph(n)
	elif GraphType.Star == type:
		graph = nx.star_graph(n)
	elif GraphType.Complete == type:
		graph = nx.complete_graph(n)
	elif GraphType.Hypercube == type:
		graph = nx.hypercube_graph(n)
	elif GraphType.Wheel == type:
		graph = nx.wheel_graph(n)
	elif GraphType.Random == type:
		graph = nx.fast_gnp_random_graph(n, 0.5)
		
	return graph


@click.group()
def cli():
	click.echo('Run program with --help option for more info')

@cli.command(name='classic')
@click.option('--graph-type', help='[path, cycle, star, complete, hypercube, wheel]')
@click.option('--n', type=int, help='1..N')
def generate_classic(graph_type, n):
	print 'Generating graph...'
	type = GraphType.fromstring(graph_type)
	Graph = generate_graph(type, int(n))
	
	output_filename = '%s_%s.graphml' % (type.value, n)
	nx.write_graphml(Graph, './%s' % output_filename)
	print 'Result is saved as "%s"' % output_filename


@cli.command(name='random')
@click.option('--n', type=int, help='1..N')
def generate_random(n):
	print 'Generating random graph...'
	Graph = generate_graph(GraphType.Random, int(n))
	
	output_filename = 'random_%s.graphml' % n
	nx.write_graphml(Graph, './%s' % output_filename)
	print 'Result is saved as "%s"' % output_filename


if __name__ == '__main__':
	cli()
