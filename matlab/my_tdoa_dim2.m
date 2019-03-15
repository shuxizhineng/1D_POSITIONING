clc;
clear all;
close all;

%% 基站坐标
MS_MAX_NUM=500;
pos=ones(MS_MAX_NUM,2);
% 基站0
pos(101,:)=[0,0];
pos(109,:)=[0,80];
% 基站列表 跨网络基站列表 对应kw1.txt
% 12_0 12_5 12_6 12_4 12_2 12_1 
% 13_0 13_1 13_5 13_4 13_6 13_3
pos(121,:)=[125.3,61.5];
pos(126,:)=[171.3,115.5];
pos(127,:)=[125.3,125.8];
pos(125,:)=[171.3,51];
pos(123,:)=[169,1.7];
pos(122,:)=[130.9,1.4];
pos(131,:)=[52.4,179.4];
pos(132,:)=[95.9,179.4];
pos(136,:)=[104.5,148.8];
pos(135,:)=[2.7,147.6];
pos(137,:)=[11.2,179.4];
pos(134,:)=[0.1,179.4];

% path='/Users/apple/Desktop/data/Dim2/1/';
% fileList=dir(fullfile(path,'anc*.txt'));  
% n=length(fileList);
dirPath='/Users/apple/Desktop/data/Dim2/';
subDirList=dir(dirPath);
for dirNum=4:length(subDirList)
    subDirName=subDirList(dirNum).name;
    path=strcat(dirPath,subDirName);
    path=strcat(path,'/')
    fileList=dir(fullfile(path,'anc*.txt'));  
    n=length(fileList);
    %所有基站
    %index=sourceId*10+(anchor+1)
    ms=[];
    %存储当前tag接收到各个基站数据进行分析
    receiveData=zeros(MS_MAX_NUM,n);
    %receiveRXLData=zeros(MS_MAX_NUM,n)-1;

    for i=1:n
        path2=strcat(path,'anc');
        path2=strcat(path2,num2str(i-1));
        filename2=strcat(path2,'.txt');

        path3=strcat(path,'src');
        path3=strcat(path3,num2str(i-1));
        filename3=strcat(path3,'.txt');

        ancData=load(filename2);
        srcData=load(filename3);
        for i=1:length(ancData)
            index=srcData(i)*10+ancData(i)+1
            if find(ms==index)
                ;
            else
                ms=[ms,index];
            end
        end
    end

    figure;
    title(path);
    for i=1:length(ms)
        plot(pos(ms(i),1),pos(ms(i),2),'*r');
        %基站编号
        netId=floor(ms(i)/10);
        ancId=ms(i)-netId*10-1;
        msId=strcat(num2str(netId),'_');
        msId=strcat(msId,num2str(ancId))
        text(pos(ms(i),1),pos(ms(i),2),msId)
        hold on
    end

        
    %保存前一刻信息
    lastTimeStatus=[];
    preconfidenceList=[];
    prePos=[];
    for i=1:n
        path1=strcat(path,'tag');
        path1=strcat(path1,num2str(i-1));
        filename1=strcat(path1,'.txt');
        path2=strcat(path,'anc');
        path2=strcat(path2,num2str(i-1));
        filename2=strcat(path2,'.txt');
        path3=strcat(path,'dis');
        path3=strcat(path3,num2str(i-1));
        filename3=strcat(path3,'.txt');
        path4=strcat(path,'src');
        path4=strcat(path4,num2str(i-1));
        filename4=strcat(path4,'.txt');
        path5=strcat(path,'rxl');
        path5=strcat(path5,num2str(i-1));
        filename5=strcat(path5,'.txt');
        
        tagData=load(filename1);
        ancData=load(filename2);
        disData=load(filename3)/100;            
        srcData=load(filename4);
        rxlData=load(filename5);
        
        %基站坐标
        msIndex=[];
        for j=1:length(ancData)
            msIndex=[msIndex,ancData(j)+1+srcData(j)*10];         %基站编号
            hold on;
        end
        
        for jj=1:length(msIndex)
            receiveData(msIndex(jj),i)=disData(jj);
            %receiveRXLData(msIndex(jj),i)=rxlData(jj);
        end
        
        %信号分析并获得可信度高的点,使用之前的数据判断可行度
        %availableDataMat=receiveData(index,:);
        maxStep=5;
        confidenceList=[];
        col=i;
        for row=1:length(msIndex)
            if col>2
%                 polyfitX=col-5:col-1;
%                 polyfitData=receiveData(msIndex(row),polyfitX);
%                 p=polyfit(polyfitX,polyfitData,1);
%                 delta=abs(p(1)*col+p(2)-receiveData(msIndex(row),col));
                delta=receiveData(msIndex(row),col)-receiveData(msIndex(row),col-1);
                if receiveData(msIndex(row),col-1)~=0
                    confidence=(maxStep-abs(delta))/maxStep;
                    confidenceList=[confidenceList,confidence];
                else
                    confidenceList=[confidenceList,-0.1];
                end
            else
                confidenceList=[confidenceList,1];
            end
        end 
        
        ok=find(confidenceList>0.8);
        if length(ok)>=3
            msIndex=msIndex(ok);
            disData=disData(ok);
        else
            ok=find(confidenceList>0);
            if length(ok)>=3
                msIndex=msIndex(ok);
                disData=disData(ok);
            else 
                msIndex=msIndex(ok);
                disData=disData(ok);
            end
        end
        
        availMsNum=length(msIndex);
        %权重,根据测距远近进行权重赋值，越近则权重越大
        K=(sum(disData)-disData)/sum(disData);
        if availMsNum<2
            fprintf("positioning error!!!");
            continue;
        elseif availMsNum==2
            syms x y
            [x,y]=solve([(x-pos(msIndex(1),1))^2+(y-pos(msIndex(1),2))^2==disData(1)^2,(x-pos(msIndex(2),1))^2+(y-pos(msIndex(2),2))^2==disData(2)^2],[x,y]);
        elseif availMsNum==3
            r=[-400,400];
            f=@(x)(((x(1)-pos(msIndex(1),1))^2+(x(2)-pos(msIndex(1),2))^2-disData(1)^2)^2*K(1)+((x(1)-pos(msIndex(2),1))^2+(x(2)-pos(msIndex(2),2))^2-disData(2)^2)^2*K(2)+((x(1)-pos(msIndex(3),1))^2+(x(2)-pos(msIndex(3),2))^2-disData(3)^2)^2*K(3));
            val=fminsearch(f,r);
            x=val(1);
            y=val(2);
        elseif availMsNum==4
            r=[-200,200];
            f=@(x)(((x(1)-pos(msIndex(1),1))^2+(x(2)-pos(msIndex(1),2))^2-disData(1)^2)^2*K(1)+((x(1)-pos(msIndex(2),1))^2+(x(2)-pos(msIndex(2),2))^2-disData(2)^2)^2*K(2)+((x(1)-pos(msIndex(3),1))^2+(x(2)-pos(msIndex(3),2))^2-disData(3)^2)^2*K(3)+((x(1)-pos(msIndex(4),1))^2+(x(2)-pos(msIndex(4),2))^2-disData(4)^2)^2*K(4));
            val=fminsearch(f,r);
            x=val(1);
            y=val(2);
        else
            r=[-200,200];
            f=@(x)(((x(1)-pos(msIndex(1),1))^2+(x(2)-pos(msIndex(1),2))^2-disData(1)^2)^2*K(1)+((x(1)-pos(msIndex(2),1))^2+(x(2)-pos(msIndex(2),2))^2-disData(2)^2)^2*K(2)+((x(1)-pos(msIndex(3),1))^2+(x(2)-pos(msIndex(3),2))^2-disData(3)^2)^2*K(3)+((x(1)-pos(msIndex(4),1))^2+(x(2)-pos(msIndex(4),2))^2-disData(4)^2)^2*K(4)+((x(1)-pos(msIndex(5),1))^2+(x(2)-pos(msIndex(5),2))^2-disData(5)^2)^2*K(5));
            val=fminsearch(f,r);
            x=val(1);
            y=val(2);
        end
        if ~isreal(x) || ~isreal(y)
            fprintf("data error!!!\n");
            continue
        end

        
        plotFlag=0;
        if availMsNum==2 && length(x)==2
            if length(preconfidenceList)>=3
                if length(prePos)~=0
                    px1=double(x(1));
                    py1=double(y(1));
                    px2=double(x(2));
                    py2=double(y(2));
                    if (px1-prePos(1))^2+(py1-prePos(2))^2<(px2-prePos(1))^2+(py2-prePos(2))^2
                        x=px1;
                        y=py1;
                    else
                        x=px2;
                        y=py2;
                    end
                    plotFlag=1; 
                end
            else
                x2=double(double(x(2)));
                y2=double(double(y(2)));
                plot(x2,y2,'oy');
                hold on;
                plotFlag=2; 
            end
            
        end
        
        x=double(x(1));
        y=double(y(1));
        
        if length(find(confidenceList>=0.8))>=3
            preconfidenceList=confidenceList;
            prePos=[x,y]
        end 

        fprintf("x:%f,y:%f,line:%d\n",x,y,i-1);
        if plotFlag==1
            plot(x,y,'og');
        elseif plotFlag==0
            plot(x,y,'ob');
        else
            plot(x,y,'oy');
        end
        text(x+0.05,y+0.05,num2str(i))
        hold on;

        for k=1:availMsNum
            xcoor=[x,pos(msIndex(k),1)];
            ycoor=[y,pos(msIndex(k),2)];
            plot(xcoor,ycoor,'--r');
        end
        pause(0.05)  
    end
    hold off;
    
    %画出接收到各个基站数据
    %获得该路径下可见基站
    index=find(sum(receiveData,2)~=0);
    row=length(index);
    figure;
    x=1:n;
    lable=[]
    for receivedMSIndex=1:length(index)
        subplot(row,1,receivedMSIndex)
        plot(receiveData(index(receivedMSIndex),:));
        title(num2str(index(receivedMSIndex)));
    end
    
    hold off; 
%     figure;
%     for receivedMSIndex=1:length(index)
%         subplot(row,1,receivedMSIndex)
%         plot(receiveRXLData(index(receivedMSIndex),:));
%         title(num2str(index(receivedMSIndex)));
%     end
end