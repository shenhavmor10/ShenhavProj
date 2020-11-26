#include "main_header.h"

#define MAX(a,b)a>b?a:b
#define MIN(a,b)a>b?b:a
#define MAX_PACKET_LENGTH 65535
#define NO_SEQ -1
#include <math.h>
#include <malloc.h>
#include <stdint.h>

//Global Header
typedef struct pcap_hdr_s {
	uint32_t magic_number; 	/* magic number */
	uint16_t version_major;	/* major version number */
	uint16_t version_minor;	/* minor version number */
	int32_t thiszone;		/* GMT to local correction */
	uint32_t sigfigs;		/* accuracy of timestamps */
	uint32_t snaplen;		/* max length of captured packets, in octets */
	uint32_t network;		/* data link type */
} GLOBAL_HDR;

//Packet Header
typedef struct pcaprec_hdr_s {
	uint32_t ts_sec; 		/* timestamp seconds */
	uint32_t ts_usec;		/* timestamp microseconds */
	uint32_t incl_len;		/* number of octets of packet saved in file */
	uint32_t orig_len;		/* actual length of packet */
} PACKET_HDR;

// Ether header strucure
typedef struct ethernet {
	UCHAR dest[6];
	UCHAR source[6];
	USHORT protocol;
} ETHER_HDR;

//Ip structure.
typedef struct iphdr
{
	unsigned char ip_header_len : 4;
	unsigned char ip_version : 4;
	unsigned char ip_tos;
	unsigned short ip_total_length;
	unsigned short ip_id;
	unsigned char ip_frag_offset : 5;
	unsigned char ip_more_fragment : 1;
	unsigned char ip_dont_fragment : 1;
	unsigned char ip_reserved_zero : 1;
	unsigned char ip_frag_offset1;
	unsigned char ip_ttl;
	unsigned char ip_protocol;
	unsigned short ip_checksum;
	unsigned int ip_src;
	unsigned int ip_dest;
} IPV4_HDR;

// Tcp structure
typedef struct tcp_header {
	unsigned short source_port;		// Source port
	unsigned short dest_port;		// Destination port
	unsigned int sequence;			// Sequence number
	unsigned int acknowledge;		// Acknowledgment number
	unsigned char ns : 1;
	unsigned char reserved_part1 : 3;
	unsigned char data_offset : 4;
	unsigned char fin : 1;
	unsigned char syn : 1;
	unsigned char rst : 1;
	unsigned char psh : 1;
	unsigned char ack : 1;
	unsigned char urg : 1;
	unsigned char ecn : 1;
	unsigned char cwr : 1;
	unsigned short window;
	unsigned short checksum;
	unsigned short urgent_pointer;
} TCP_HDR;

// UDP header
typedef struct udp_header{
	uint16_t source_port;	// Source port
	uint16_t dest_port; 	// Destination port
	uint16_t len;			// Datagram length
	uint16_t crc;			// Checksum
} UDP_HDR;

//layer struct.
typedef struct layer
{
	char* payload;
	int size;
}LAYER;

//enum of errors.
typedef enum ERROR_STATUS
{
	DUP_ACK, KEEP_ALIVE, KEEP_ALIVE_ACK, PREV_SEGMENT_LOST,
	OUT_OF_ORDER, RETRANSMISSION, FAST_RETRANSMISSION, SPURIOUS_RETRANSMISSION,
	WINDOW_FULL, WINDOW_UPDATE, ZERO_WINDOW, ZERO_WINDOW_PROBE, ZERO_WINDOW_PROBE_ACK, NON_ERROR
} ERROR_STATUS;

//struct of node in the packet list.
typedef struct linearLinkedList
{
	LAYER* layer1;
	LAYER* layer2;
	LAYER* layer3;
	LAYER* layer4;
	unsigned long unique_value;
	char* data;
	struct PACKET_LIST* next;
	struct PACKET_LIST* prev;
	ERROR_STATUS error_type;
	int packet_id;
}PACKET_LIST, * PACKET_LIST_PTR;

//struct of node in the stream list.
typedef struct stream_ptr
{
	unsigned int unique_value;
	struct STREAM_LIST* next;
	PACKET_LIST_PTR packets;
}STREAM_LIST, * STREAM_LIST_PTR;

//creating new data type that gets pointer to packet and returns int.
typedef int(*function)(PACKET_LIST_PTR);

void create_eth(ETHER_HDR*);
void create_ip(IPV4_HDR*);
void create_tcp(TCP_HDR*);
void create_udp(UDP_HDR*);
void print_mac(UCHAR*, BOOL);
unsigned long elegant_pair(unsigned int, unsigned int);
unsigned long unique_number_generator(unsigned int, unsigned int, unsigned short, unsigned short);

void init_packet_list(PACKET_LIST_PTR*);
void init_stream_list(STREAM_LIST_PTR*);
int insert_after_packet(PACKET_LIST_PTR*, PACKET_LIST_PTR);
int insert_after_stream(STREAM_LIST_PTR* node, unsigned int value);
STREAM_LIST_PTR search_stream_value(STREAM_LIST_PTR, unsigned int); 
PACKET_LIST_PTR lastPacket(PACKET_LIST_PTR);
int num_of_streams(STREAM_LIST_PTR);
int num_of_packets(PACKET_LIST_PTR);
int read_packet(FILE*, GLOBAL_HDR, PACKET_LIST_PTR*);
int insert_packet(STREAM_LIST_PTR*, PACKET_LIST);
void print_packet_info(PACKET_LIST_PTR);
int cmp(unsigned int, unsigned int, int);
PACKET_LIST_PTR prev_packet_finder(PACKET_LIST_PTR, int);
int calc_data_length(PACKET_LIST_PTR);
int dup_ack(PACKET_LIST_PTR);
int keep_alive(PACKET_LIST_PTR);
int keep_alive_ack(PACKET_LIST_PTR);
int prev_segment_lost(PACKET_LIST_PTR);
int out_of_order(PACKET_LIST_PTR);
int retransmission(PACKET_LIST_PTR);
int fast_retransmission(PACKET_LIST_PTR);
int spurious_retransmission(PACKET_LIST_PTR);
int window_full(PACKET_LIST_PTR);
int window_update(PACKET_LIST_PTR);
int zero_window(PACKET_LIST_PTR);
int zero_window_probe(PACKET_LIST_PTR);
int zero_window_probe_ack(PACKET_LIST_PTR);
void init_errors_array(function[]);
int save_for_graph(FILE *, int, int);

typedef struct
{
	unsigned int packet_id;
	unsigned int error_type;
}PACKET_INFO;