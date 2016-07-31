using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;

public class PacketQueue
{	
	// 패킷 저장 정보.
	struct PacketInfo
	{
		public int	offset;
		public int 	size;
	};
	
	//
	private MemoryStream 		m_streamBuffer;
	
	private List<PacketInfo>	m_offsetList;
	
	private int					m_offset = 0;

	
	// 
	public PacketQueue()
	{
		m_streamBuffer = new MemoryStream();
		m_offsetList = new List<PacketInfo>();
	}
	
	// 
	public int Enqueue(byte[] data, int size)
	{
		PacketInfo	info = new PacketInfo();
	
		info.offset = m_offset;
		info.size = size;
			
		// 패킷 저장 정보를 보존.
		m_offsetList.Add(info);
		
		// 패킷 데이터를 보존.
		m_streamBuffer.Position = m_offset; // 오프셋 저장위치 설정
		m_streamBuffer.Write(data, 0, size); // 저장위치 설정된곳부터 사이즈까지
		m_streamBuffer.Flush(); // 메모리스트림에 반영하기
		m_offset += size; // 오프셋 사이즈만큼 증가.
		
		return size;
	}

    // byte[] buffer = new byte[1400];
    public int Dequeue(ref byte[] buffer, int size) {
         

        if (m_offsetList.Count <= 0) {
            //.Log("PacketQueue::Receive(ref byte[] buffer, if (m_offsetList.Count <= 0) , return - 1");
            return -1;
		}
		
        // 0은 초기값 , 마지막은 최시값 , 그래서 항상 0부터 값을 뽑음
		PacketInfo info = m_offsetList[0];
	
		// 버퍼에서 해당하는 패킷 데이터를 가져옵니다.
		int dataSize = Math.Min(size, info.size);

        // 가져오려는 오프셋위치 설정
        m_streamBuffer.Position = info.offset;
        
        // 버퍼의 포지션에서 오프셋위치부터 데이터사이즈까지 데이터를 담는다. 
		int recvSize = m_streamBuffer.Read(buffer, 0, dataSize);
		
		// 큐 데이터를 가져왔으므로 선두 요소를 삭제.
		if (recvSize > 0) {
			m_offsetList.RemoveAt(0);
		}
		
		// 모든 큐 데이터를 가져왔을 때는 스트림을 클리어해서 메모리를 절약합니다. 
		if (m_offsetList.Count == 0) {
			Clear();
			m_offset = 0;
		}
		
		return recvSize;
	}
	
	public void Clear()
	{
        // 메모리스트림의 메모리주소값들을 가져오는듯하다.
		byte[] buffer = m_streamBuffer.GetBuffer();

        // 메모리위치들을 0으로 초기화시킨다.
		Array.Clear(buffer, 0, buffer.Length);
		
        //오프셋 포지션을 0으로 초기화
		m_streamBuffer.Position = 0;

        // 메모리스트림의 길이를 0으로 설정
		m_streamBuffer.SetLength(0);
	}
}

